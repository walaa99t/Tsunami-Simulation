using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionrotfix : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<Rigidbody>().isKinematic = true;
        //Debug.Log(this.GetComponent<Transform>().position);
    }

    private void OnTriggerEnter(Collider other)
    {         
        if (other.tag != "collider")
        {
                manager.TSRot.Add(other);
        }
    }
}
