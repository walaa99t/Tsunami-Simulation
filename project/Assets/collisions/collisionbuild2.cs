using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionbuild2 : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        this.GetComponent<Rigidbody>().isKinematic = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "collider")
        {      
            manager.TSBuild.Add(other);
        }
    }
}
