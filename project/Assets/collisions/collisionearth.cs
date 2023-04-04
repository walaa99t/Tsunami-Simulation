using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionearth : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<Rigidbody>().isKinematic = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "collider")
        {
                other.GetComponent<Transform>().position += new Vector3(0.0f, 0.5f, 0.0f);
        }
    }
}
