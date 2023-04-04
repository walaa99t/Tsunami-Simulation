using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionrot : MonoBehaviour
{
    private manager Manager;

    private int k1 = 0;
    private Renderer rend;

    //public GameObject rwall;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody>().isKinematic = true;
        rend = GetComponent<Renderer>();
        var materialColor = rend.material.color;
        materialColor.a = 1.0f;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "collider")
        {
            if (other.GetComponent<Transform>().position.x < 20 & other.GetComponent<Transform>().position.y < 20)
            {
                        manager.TS.Add(other);
            }
        }
    }


    private int secondsToDestroy = 5;
    void Awake(){
        Destroy(this,secondsToDestroy);
    }
}
