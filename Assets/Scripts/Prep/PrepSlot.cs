﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a slot on the Prep table.
/// AKA When the chef cooks the food and places the food on this table.
/// 
/// @author ShifatKhan
/// </summary>
public class PrepSlot : Interactable
{
    public bool occupied { get; private set; }

    public FoodSlot foodSlot;
    public GameObject foodGameObject;

    private RadialProgressBar progressBar;
    private GameObject progressBarGameObject;

    private float timer;
    private bool cooking = false;

    private PrepTable prepTable;

    private AudioSource audioSource;
    public AudioClip bell;

    private void Awake()
    {
        progressBar = GetComponentInChildren<RadialProgressBar>();
        progressBarGameObject = progressBar.gameObject;
        prepTable = GetComponentInParent<PrepTable>();

        audioSource = GetComponent<AudioSource>();
    }

    public override void Start()
    {
        occupied = false;
        progressBarGameObject.SetActive(false);

        playerInput = PlayerInputManager.instance;
    }

    public override void Update()
    {
        //if (playerInput.pickDropInput && playerInRange)
        //{
        //    TakeFood();
        //}

        if (cooking)
        {
            timer += Time.deltaTime;
            progressBar.progress = (int)((timer / foodSlot.prepTime) * 100);
        }
    }

    public void PrepFood(FoodSlot food)
    {
        foodSlot = food;
        
        StartCoroutine(PrepTime());
    }

    IEnumerator PrepTime()
    {
        occupied = true;
        progressBarGameObject.SetActive(true);

        cooking = true;

        audioSource.loop = true;
        audioSource.Play();

        yield return new WaitForSeconds(foodSlot.prepTime);

        audioSource.Stop();
        audioSource.PlayOneShot(bell);

        cooking = false;
        timer = 0;
        progressBarGameObject.SetActive(false);

        // Food is ready.
        foodGameObject = Instantiate(foodSlot.food.foodPrefab);
        foodGameObject.transform.SetParent(transform);
        foodGameObject.transform.localPosition = Vector3.zero;
        
        foodGameObject.AddComponent<Food>();
        foodGameObject.GetComponent<Food>().SetFood(foodSlot.food);
        foodGameObject.GetComponent<Food>().tableNumber = foodSlot.tableNumber;
    }

    public void TakeFood()
    {
        // Don't do anything if it is still cooking.
        if (cooking)
            return;

        ResetSlot();
        prepTable.CheckForFreeSlots();
    }

    private void ResetSlot()
    {
        occupied = false;
        cooking = false;
        timer = 0;
        progressBarGameObject.SetActive(false);
        foodGameObject = null;
        foodSlot = null;
    }

    /// <summary>
    /// Checks if this prep slot's food was taken or not.
    /// </summary>
    [System.Obsolete("This will be deleted as we are using TakeFood() instead.", false)]
    public void UpdatePrepSlot()
    {
        if (cooking)
            return;

        bool foodFound = false;

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Food"))
                foodFound = true;
        }

        // If this slot has no food, then we reset the slot.
        if (!foodFound)
            ResetSlot();
    }
}
