﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public Transform objectPosition;
    Transform mopPosition;
    Transform foodPosition;
    Rigidbody objectRigidBody;
    BoxCollider objectBoxCollider;
    Outline outlineScript;

    public bool pickedUp { get; private set; }

    private void Awake()
    {
        foodPosition = GameObject.FindGameObjectWithTag("Player").transform.Find("Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/PickupFood").transform;
        mopPosition = GameObject.FindGameObjectWithTag("Player").transform.Find("Armature/Hips/Spine/Chest/Right shoulder/Right arm/Right elbow/Right wrist/PickupMop").transform;

        objectRigidBody = GetComponent<Rigidbody>();
        objectBoxCollider = GetComponent<BoxCollider>();
        outlineScript = GetComponent<Outline>();

        //Disable outline script when starting the game
        disableEnableOutline(false);

        //Sets position of object on the hand of the character
        if(gameObject.CompareTag("Mop"))
        {
            objectPosition = mopPosition;
        }
        else //its a food or water jug
        {
            objectPosition = foodPosition;
        }
    }

    public void PickObjectUp()
    {
        objectBoxCollider.enabled = false;
        objectRigidBody.useGravity = false; //No gravity on object when holding it

        //Free rotation and position, so it stops moving when picked
        objectRigidBody.freezeRotation = true; 
        objectRigidBody.constraints = RigidbodyConstraints.FreezePosition;
 
        this.transform.position = objectPosition.position;
        this.transform.parent = objectPosition;
        this.transform.localRotation = Quaternion.identity; //object is held upright
        this.transform.localPosition = Vector3.zero;

        pickedUp = true;
 
        //Disable outline script
        disableEnableOutline(false);
    }

    public void PlaceObjectDown()
    {
        this.transform.parent = null;
        objectBoxCollider.enabled = true;
        objectRigidBody.useGravity = true;
        objectRigidBody.freezeRotation = false; 
        objectRigidBody.constraints = RigidbodyConstraints.None;

        pickedUp = false;

        //Re-enable outline script
        disableEnableOutline(true);
    }

    //Disable/Enable outline script
    void disableEnableOutline(bool value)
    {
        if(outlineScript != null)
        {
            outlineScript.enabled = value;
        }
    }
}
