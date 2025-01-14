﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckNearbyInteraction : MonoBehaviour
{
    public bool holdingObject = false;
    public static bool holdingWaterJug = false;
    public GameObject currentObjectHold; //Reference to current object being held
    PlayerInputManager playerInput;
    PlayerAnimation playerAnimation;

    Collider previousNearestObject;

    Collider nearest;

    Collider currentOutlined;

    float radius = 3.5f;

    private void Start()
    {
        playerInput = PlayerInputManager.instance;
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update() 
    {
        NearbyObjects();  
    }

    void NearbyObjects()
    {
        //Nearby objects within a radius of 3.5
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, radius);

        //Calculate nearest object
        nearest = nearbyObjects[0];
        foreach (Collider objectNear in nearbyObjects)
        {
            if(objectNear.GetComponent<PickUp>() != null && Vector3.Distance(transform.position, objectNear.transform.position) < Vector3.Distance(transform.position, nearest.transform.position))
            {
                nearest = objectNear;
            }
        }

        //If nearest object contains Outline script then highlight it
        if(holdingObject == false && nearest.transform.gameObject.GetComponent<Outline>() != null) 
        {
            currentOutlined = nearest; 
            currentOutlined.transform.gameObject.GetComponent<Outline>().enabled = true;

            if(previousNearestObject == null)
            {
                previousNearestObject = currentOutlined; 
            }
        }

        turnOffOutline(); //Checks if the highlight of the object should be turned off

        //Picks up/drops down the object
        pickDropCheck();
    }

    void turnOffOutline()
    {
        //Case 1: object goes too far away, so do not highlight it anymore
        if(Vector3.Distance(transform.position, nearest.transform.gameObject.transform.position) > radius)
        {
            if(currentOutlined != null)
                currentOutlined.transform.gameObject.GetComponent<Outline>().enabled = false;
            previousNearestObject = currentOutlined;   
        }
        
        //Case 2: another object becomes closer, so unhighlight the object that was already highlighted
        if((previousNearestObject != nearest) && (previousNearestObject != null))
        {
            if(previousNearestObject.transform.gameObject.GetComponent<Outline>() != null)
                previousNearestObject.transform.gameObject.GetComponent<Outline>().enabled = false;
            previousNearestObject = currentOutlined;
        }
    }

    //Users presses 'f' to pick up or drop down an object
    void pickDropCheck()
    {
        if(playerInput.pickDropInput == true)
        {
            if(holdingObject == false)
            {
                PickUp pickUpScript = nearest.GetComponent<PickUp>();
                if(pickUpScript != null) //Object must contain PickUp script
                {


                    // Check if we're taking food from a prep table.
                    PrepSlot prepSlot = pickUpScript.GetComponentInParent<PrepSlot>();
                    if (prepSlot != null) // Object is a prepped food
                    {
                        prepSlot.TakeFood();
                    }

                    // Check if we're holding a water jug
                    if(pickUpScript.CompareTag("Water Jug"))
                    {
                        holdingWaterJug = true;
                    }

                    pickUpScript.PickObjectUp();
                    currentObjectHold = nearest.gameObject;
                    holdingObject = true;
                    AudioManager.PlayPlayerPickup();

                    PlayAnimation();
                }
            }

            //Drop object
            else
            {
                ObjectDown();
            }
        }
    }

    public void ObjectDown()
    {
        AudioManager.PlayPlayerDrop();
        playerAnimation.FoodDroppedAnimation();
        holdingWaterJug = false;
        currentObjectHold.GetComponent<PickUp>().PlaceObjectDown();
        holdingObject = false;
        currentObjectHold = null;
    }

    //Returns current object held
    public GameObject getHeldObject()
    {
        return currentObjectHold;
    }

    void PlayAnimation()
    {
        if(currentObjectHold.CompareTag("Mop"))
        {
            playerAnimation.MopAnimation();
        }
        else //its a water jug or food
        {
            playerAnimation.FoodPickedAnimation();
        }
    }
}
