﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(UnityEngine.AI.NavMeshAgent))]
public class NpcMoveToTable : MonoBehaviour
{

    public int tableNumber;
    private UnityEngine.AI.NavMeshAgent agent;
    private Animator animator;

    public GameObject idle;
    public GameObject walking;
    public GameObject sitting;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        EnableWalkAnimation();
    }

    void Update()
    {
    }

    public void DisableAIMovement()
    {
        agent.isStopped = true;
    }

    public void SetTableNumber(int tableNumber)
    {
        this.tableNumber = tableNumber;
        SetAgentTableDestination();
    }

    void SetAgentTableDestination()
    {
        Table[] tables = TableManager.Instance.tables;
        agent.destination = tables[tableNumber].transform.position;
    }

    public void EnableWalkAnimation()
    {
        animator.SetBool("walking", true);
        animator.SetBool("sitting", false);
    }

    public void EnableSittingAnimation()
    {
        animator.SetBool("walking", false);
        animator.SetBool("sitting", true);
    }
}
