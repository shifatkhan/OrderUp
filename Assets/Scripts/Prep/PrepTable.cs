﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class takes care of the food queue in the prep table.
/// When a food is served, it will start preparing the next food in queue.
/// 
/// @author ShifatKhan
/// </summary>
public class PrepTable : Interactable
{
    [Header("PrepTable vars")]
    [SerializeField]
    private List<Transform> prepSlots;

    [SerializeField]
    private MemoryData memory;

    [SerializeField]
    private Queue<FoodSlot> foodQueue = new Queue<FoodSlot>();

    [Header("UI")]
    [SerializeField]
    private Text queueText;
    [SerializeField]
    private Transform memoryUI;

    private AudioSource audioSource;

    void Awake()
    {
        prepSlots = new List<Transform>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Start()
    {
        base.Start();

        foreach (Transform child in transform)
        {
            if(child.CompareTag("PrepSlot"))
                prepSlots.Add(child);
        }

        UpateQueueText();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void OnInteract()
    {
        base.OnInteract();

        QueueFoods();
    }

    private void QueueFoods()
    {
        List<FoodSlot> tempFoodsInMemory = memory.GetFoodsMemorized().ToList();

        // GET FOODS
        if (tempFoodsInMemory.Count == 0)
            return;
        else
        {
            foreach (FoodSlot f in tempFoodsInMemory)
            {
                foodQueue.Enqueue(f);
            }
        }

        memory.Clear();
        CheckForFreeSlots();

        UpateQueueText();
    }

    public void CheckForFreeSlots()
    {
        bool free = false;

        // Check if there's a free slot.
        for (int i = 0; i < prepSlots.Count; i++)
        {
            PrepSlot current = prepSlots[i].GetComponent<PrepSlot>();
            if (!current.occupied)
            {
                free = true;
                break;
            }
        }

        // Return void is there are no free slots.
        if (!free)
            return;

        // PREP FOODS
        for (int i = 0; i < prepSlots.Count && foodQueue.Count > 0; i++)
        {
            PrepSlot current = prepSlots[i].GetComponent<PrepSlot>();
            if (!current.occupied)
            {
                audioSource.Play();
                current.PrepFood(foodQueue.Dequeue());
                UpateQueueText();
            }
        }
    }

    public void UpateQueueText()
    {
        queueText.text = foodQueue.Count.ToString();
    }

    /// <summary>
    /// Checks if this prep slot's food was taken or not.
    /// This is a fix for when the player stands in from of a prepSlot, 
    /// but takes the food from a different prepSlot.
    /// </summary>
    [System.Obsolete("This will be deleted as we are using PrepSlot.TakeFood() instead.", false)]
    public void UpdatePrepSlots()
    {
        foreach (Transform prepSlot in prepSlots)
        {
            PrepSlot current = prepSlot.GetComponent<PrepSlot>();
            current.UpdatePrepSlot();
        }
    }
}
