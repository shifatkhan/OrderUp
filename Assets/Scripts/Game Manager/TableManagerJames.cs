﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a temporary test class that will activate the tables randomly to simulate customers.
/// 
/// @author: ShifatKhan, Nhut Vo
/// </summary>
public class TableManagerJames : MonoBehaviour
{
    public Table[] tables;

    private float spawnTime = 0f;

    [SerializeField]
    private float spawnRate = 2f;


    private static TableManagerJames _instance;

    public static TableManagerJames Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<TableManagerJames>();
                if (_instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(TableManagerJames).Name;
                    _instance = go.AddComponent<TableManagerJames>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        spawnTime = Time.time + spawnRate;

        // Assign a table number to each table.
        for (int i = 0; i < tables.Length; i++)
        {
            tables[i].tableNumber = i;
        }
    }

    void Update()
    {

        //// Check if we should spawn.
        //if (Time.time > spawnTime)
        //{
        //    spawnTime += spawnRate;

        //    int i = Random.Range(0, tables.Length);
        //    // Check if table is occupied
        //    if (tables[i].tableState == TableState.Empty)
        //    {
        //        tables[i].EnableCustomers();
        //    }
        //}
    }

    
}