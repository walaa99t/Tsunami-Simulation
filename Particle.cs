using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public Vector3 x;
    public Vector3 v, f;
    public float rho;
    public float p;
    public int id;

    public Particle(float _x, float _y,float _z)
    {
        this.x=new Vector3(_x,_y,_z);
        this.v = new Vector3(0.0f ,0.0f,0.0f);
        this.f = new Vector3(0.0f ,0.0f,0.0f);
        this.rho = 0.0f;
        this.p = 0.0f;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}