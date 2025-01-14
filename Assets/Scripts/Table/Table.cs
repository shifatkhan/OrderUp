﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/// <summary>
/// This Table class will be keeping track of a table's state and the loop.
/// The loop consists of the TableState.
/// 
/// This script inherits from Interactable, which allows Player to interact on it.
/// 
/// @author: ShifatKhan, Nhut Vo, Thanh Tung Nguyen
/// </summary>
public enum TableState
{
    Available,
    Occupied,
    ReadyToOrder,
    WaitingForFood,
    Eating,
    ReadyToPay
}
public class Table : Interactable
{
    //****************** TABLE INFO ******************//
    [Header("Table info")]
    public int tableNumber;
    public List<GameObject> chairs { get; private set; }
    public bool[] occupiedChairs { get; private set; }

    private GameObject foodOnTable;

    //****************** SCORING ******************//
    [Header("Order")]
    [Range(0,1)]
    [SerializeField] private float baseTip = 0.15f; // Tip to add to totalPay
    [Min(1)]
    [SerializeField] private float bonusMultiplier =  1.2f; // Bonus to multiply on baseTip
    [SerializeField] private float totalPay = 0; // Total amount to be paid
    
    //****************** ORDERING ******************//
    public TableState tableState { get; private set; }
    public float maxOrderTime = 20f;
    public float minOrderTime = 5f;

    public int maxOrderAmount = 1;
    public int minOrderAmount = 9;

    //****************** MEMORY ******************//
    [Header("Other")]
    [SerializeField]
    private MemoryData memory;
    [SerializeField]
    private GameEvent memoryEvent;

    private FoodFactory foodFactory;
    private List<FoodSlot> allOrders;
    private List<FoodSlot> currOrders = new List<FoodSlot>();

    //****************** UI ******************//
    [SerializeField] private Transform T_memoryUI;
    private MemoryUI memoryUI;
    private PatienceMeter patienceManager;
    private WaterPourable waterManager;
    private Animator tableStateAnim;
    private Image foodOrdersImg;

    public override void Start()
    {
        base.Start();

        chairs = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.tag == "Chair")
            {
                chairs.Add(child.gameObject);
            }
        }

        occupiedChairs = new bool[chairs.Count];
        tableState = TableState.Available;

        // find components
        foodFactory = GameObject.FindGameObjectWithTag("Food Factory").GetComponent<FoodFactory>();
        patienceManager = GetComponent<PatienceMeter>();
        waterManager = GetComponent<WaterPourable>();
        tableStateAnim = transform.Find("Table State UI/Bubble/Table State").GetComponent<Animator>();
        foodOrdersImg = transform.Find("Table State UI/Bubble/Food Orders").GetComponent<Image>();
        memoryUI = T_memoryUI.GetComponent<MemoryUI>();

        this.updateStateInUI();
    }

    public override void Update()
    {
        base.Update();

        if (patienceManager.patience <= 0)
            ResetTable();
    }

    public override void OnInteract()
    {
        base.OnInteract();

        if (tableState == TableState.ReadyToOrder)
        {
            // customer's order is taken
            if (!CheckNearbyInteraction.holdingWaterJug)
            {
                memoryUI.OpenUIForOrders(this, allOrders);
            }
            
            // note Waiting() now called at bottom when player takes an order
        }
        else if (tableState == TableState.ReadyToPay)
        {
            if (!CheckNearbyInteraction.holdingWaterJug)
            {
                Pay();
            }
        }
    }

    public void EnableCustomers()
    {
        tableState = TableState.Occupied;

        // customers start drinking water
        patienceManager.SetActive(true);
        patienceManager.resetPatience();
        waterManager.setActive(true);
        waterManager.waterFilled();
        // TODO adjust difficulty by calling one of waterManager's method

        updateStateInUI();
        StopAllCoroutines(); // This fixes the glitch where a table places multiple orders.
        StartCoroutine(OrderFood(Random.Range(minOrderTime, maxOrderTime)));
    }

    IEnumerator OrderFood(float orderTime)
    {
        yield return new WaitForSeconds(orderTime);
        tableState = TableState.ReadyToOrder;

        // Choose something to order.
        int numOfFoodOrders = Random.Range(minOrderAmount, maxOrderAmount+1); // 1 to 8
        allOrders = new List<FoodSlot>(numOfFoodOrders);
        for (int i = 0; i < numOfFoodOrders; i++)
        {
            allOrders.Add(new FoodSlot(foodFactory.GetRandomFood(), tableNumber));
            totalPay += allOrders[i].price;
        }

        updateStateInUI();
    }

    public void Waiting()
    {
        tableState = TableState.WaitingForFood;
        memoryEvent.Raise();

        updateStateInUI();
    }

    public void Eating()
    {
        tableState = TableState.Eating;
        patienceManager.SetActive(false); // stop depleting patience when eating

        StartCoroutine(EatingCo(Random.Range(minOrderTime, maxOrderTime)));

        updateStateInUI();
    }

    IEnumerator EatingCo(float eatingTime)
    {
        yield return new WaitForSeconds(eatingTime);

        // Finished eating. If all orders are met, they pay
        // else, they will want to order more
        if (allOrders.Count == 0)
        {
            tableState = TableState.ReadyToPay;
        }
        else
            tableState = TableState.ReadyToOrder;

        patienceManager.SetActive(true); // start depleting patience again

        updateStateInUI();
    }

    public void Pay()
    {
        tableState = TableState.Available;
		
        patienceManager.SetActive(false);
        waterManager.setActive(false);

        AudioManager.PlayPayAudio();

        ResetTable();

        CalculateTotalPay();
        updateStateInUI();
    }

    public void AddBaseTip(float amount)
    {
        // Base tip can't go above 1.
        if (amount <= 0 || amount + baseTip > 1)
            return;

        baseTip += amount;
    }

    public void SubstractBaseTip(float amount)
    {
        // Base tip can't go below 0.
        if (amount <= 0 || baseTip - amount < 0)
            return;

        baseTip -= amount;
    }

    public void AddBonusMultiplier(float amount)
    {
        if (amount <= 0)
            return;

        bonusMultiplier += amount;
    }

    public void SubstractBonusMultiplier(float amount)
    {
        // Base tip can't go below 1.
        if (amount <= 0 || bonusMultiplier - amount < 1)
            return;

        bonusMultiplier -= amount;
    }

    /// <summary>
    /// This should be called at the end.
    /// AKA when the table is going to pay "Pay()"
    /// 
    /// We also update the score in Score Manager.
    /// </summary>
    /// <returns>totalPay: the final pay</returns>
    public float CalculateTotalPay()
    {
        // Calculate total tip
        float totalTip = baseTip + (baseTip * bonusMultiplier);

        // Calculate total pay
        totalPay = totalTip * totalPay;

        // Update score
        ScoreManager.AddScore(totalPay);
        GameManager.customersPaid++;
        GameManager.totalTipPercent += totalTip;

        return totalPay;
    }

    public void ResetTable()
    {
        tableState = TableState.Available;
        allOrders.Clear();
        currOrders.Clear();

        for (int i = 0; i < occupiedChairs.Length; i++)
        {
            occupiedChairs[i] = false;

            // If we find "NpcMoveToTable", then there's a customer sitting on this chair.
            NpcMoveToTable customer = chairs[i].GetComponentInChildren<NpcMoveToTable>();
            if (customer != null)
                Destroy(customer.gameObject);
        }

        Transform pickup = transform.Find("PickupObject");
        foreach (Transform food in pickup)
        {
            Destroy(food.gameObject);
        }

        // reset table state UI
        patienceManager.SetActive(false);
        patienceManager.ResetPatience();
        waterManager.setActive(false);
        waterManager.waterFilled();

        updateStateInUI();
    }

    public override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);

        if (other.CompareTag("Food"))
        {
            // Check if a Food was placed on the table.
            Food food = other.GetComponent<Food>();
            if (food == null || currOrders.Count == 0)
                return;

            // Check if the food placed was meant for this table number.
            for (int i=0; i < currOrders.Count; i++)
            {
                if (food.foodName == currOrders[i].foodName)
                {
                    // Correctly delivered the food.
                    other.GetComponent<PickUp>().objectPosition = transform.Find("PickupObject");
                    other.GetComponent<PickUp>().PickObjectUp();
                    currOrders.RemoveAt(i);

                    patienceManager.increPatience(0.1f);
                    GameManager.ordersServed++;

                    // only when they receive all their current orders will they start eating
                    // else, they will keep waiting
                    if (currOrders.Count == 0)
                        Eating();
                    else
                        updateStateInUI();

                    break;
                }
            }
            
        }
        else if (other.CompareTag("Customer"))
        {
            if (other.GetComponent<NpcMoveToTable>().tableNumber == this.tableNumber)
            {
                this.EnableCustomers();
                for (int i = 0; i < occupiedChairs.Length; i++)
                {
                    if (!occupiedChairs[i])
                    {
                        other.GetComponent<NpcMoveToTable>().EnableSittingAnimation();
                        other.GetComponent<NpcMoveToTable>().DisableAIMovement();
                        other.GetComponent<Collider>().enabled = false;
                        Destroy(other.GetComponent<Rigidbody>());

                        // Sit customers
                        other.transform.parent = chairs[i].transform;
                        other.transform.position = chairs[i].GetComponent<Chair>().sitGO.transform.position;

                        Vector3 lookAt = transform.position;
                        lookAt.y = other.transform.position.y;
                        other.transform.LookAt(lookAt);
                        occupiedChairs[i] = true;
                        break;
                    }
                }
            }
        }
    }

    private void updateStateInUI()
    {
        tableStateAnim.SetBool("ReadyToOrder", false);
        tableStateAnim.SetBool("AwaitingFood", false);
        tableStateAnim.SetBool("ReadyToPay", false);
        foodOrdersImg.gameObject.SetActive(false);

        switch (this.tableState)
        {
            case TableState.ReadyToOrder:
                tableStateAnim.SetBool("ReadyToOrder", true); break;
            case TableState.WaitingForFood:
                if (currOrders.Count >= 1)
                {
                    foodOrdersImg.sprite = currOrders[0].food.foodImage;
                    foodOrdersImg.gameObject.SetActive(true);
                }
                break;
            case TableState.ReadyToPay:
                tableStateAnim.SetBool("ReadyToPay", true); break;
            default:
                break;
        }
    }

    public void displayHeart()
    {
        StartCoroutine(patienceManager.displayHeart());
    } 

    public void TakeOrder(string foodName)
    {
        for (int i = 0; i < allOrders.Count; i++)
        {
            if (allOrders[i].foodName == foodName)
            {
                // add taken order to memory and remove it from customer's orders
                memory.AddFood(allOrders[i]);
                currOrders.Add(allOrders[i]);
                allOrders.RemoveAt(i);

                break; // break in case theres a duplicate order
            }
        }

        Waiting();
        memoryUI.UpdateOrdersTakenUI();
    }
}

