using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
//using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
public class manager : MonoBehaviour
{
    private Particle P;
    private ArrayList Particles = new ArrayList();
    private Vector2 G = new Vector3(0.0f, -9.81f, 0.0f);

    private float REST_DENS = 1f; //5000f
    private float GAS_CONST = 2000.0f;

    private float H;
    private float HSQ;
    private float MASS = 4f;
    private float VISC = 0.1f;
    private float DT = 0.007f; //0.0007f
    private float VIEW_HEIGHT = 10;
    private float VIEW_WIDTH = 10;
    private float VIEW_DEPTH = 10;

    private float POLY6;
    private float SPIKY_GRAD;
    private float VISC_LAP;

    private float EPS;
    private float BOUND_DAMPING;
    float x = 0, z = 0, y = 0;
    private static int Pamount = 1000;
    private float particleRadius = 1f;
    private Transform trans;
    private Vector3 scale;
    float timer = 0f;
    GameObject water;
    GameObject w;
    private GameObject tsunami;
    private GameObject tsumnamiInit;
    float DX, DY, DZ;
    private GameObject[] objects;
    private Dictionary<int, List<int>> _hashGrid = new Dictionary<int, List<int>>();
    private int maximumParticlesPerCell = 500;

    private int dimensions = 30;
    private float[] densities;
    private float[] pressures;
    private Vector3[] forces;
    private Vector3[] velocities;
    private ComputeBuffer _particleColorPositionBuffer;
    private ComputeBuffer _argsBuffer;
    private Vector3Int[] nearCells;
    private Dictionary<int, List<int>> nei;
    private ArrayList Cel;
    private Dictionary<int, List<int>> Neighbours = new Dictionary<int, List<int>>();
    private float radius2;
    private float radius3;
    private float radius4;
    private float radius5;
    public GameObject[] colliders;
    public GameObject UIPanel;
    public GameObject GamePanel;
    private int temperature=25;
    private string cam;
    private float tsX;
    private float tsY;
    private float tsZ;
    public GameObject upCam;
    public GameObject frontCam;
    public GameObject left3dCam;
    public GameObject right3dCam;
    public static List<Collider> TS = new List<Collider>();
    public static List<Collider> TSRot = new List<Collider>();
    public static List<Collider> TSBuild = new List<Collider>();
    public static List<Collider> TSWallBack = new List<Collider>();
    public static float Den;
    public static float pres;
    public static float x2 = float.PositiveInfinity;
    public static float y2 = float.PositiveInfinity;
    public static float z2 = float.PositiveInfinity;
    private int heightStop;

    void Awake()
    {
        H = 2 * particleRadius;
        HSQ = H * H;
        POLY6 = 315f / (float)(64 * Math.PI * Math.Pow(H, 9));
        SPIKY_GRAD = -45f / (float)(Math.PI * Math.Pow(H, 6));
        VISC_LAP = 45f / (float)(Math.PI * Math.Pow(H, 6));
        EPS = (float.Epsilon) / 4; // H/4;
        BOUND_DAMPING = -0.5f; //0.5f
        Cel = new ArrayList();
        radius2 = particleRadius * particleRadius;
        radius3 = radius2 * particleRadius;
        radius4 = radius3 * particleRadius;
        radius5 = radius4 * particleRadius;
        if (GamePanel.activeSelf)
        {
            Debug.Log("Pamount is "+ Pamount);
            setCam();
            setTs();
            addParticle();
            InitNeighbourHashing();
        }
    }

    public float TsX
    {
        get => tsX;
        set => tsX = value;
    }

    public float TsY
    {
        get => tsY;
        set => tsY = value;
    }

    public float TsZ
    {
        get => tsZ;
        set => tsZ = value;
    }

    private void setTs()
    {
        tsunami= Resources.Load("rWall2") as GameObject;
        Vector3 pos = new Vector3(tsX, tsY, tsZ);
        tsumnamiInit = Instantiate(tsunami, pos, tsunami.transform.rotation);
        if (tsX >= 0 & tsX <= 8.5)
        {
            if (tsY >= 1 & tsY <= 4.5)
            {
                heightStop = 21;
            }
            else
            {
                heightStop = 18;
            }
        }
        else
        {
            if (tsY >= 1 & tsY <= 4.5)
            {
                heightStop = 15;
            }
            else
            {
                heightStop = 12;
            }
        }
        if (x2 != float.PositiveInfinity & y2 != float.PositiveInfinity & z2 != float.PositiveInfinity)
        {
            tsunami= Resources.Load("rWall2") as GameObject;
            Vector3 pos2 = new Vector3(x2, y2, z2);
            Debug.Log("another tsunami in position "+ pos2);
            tsumnamiInit = Instantiate(tsunami, pos2, tsunami.transform.rotation);
        }

    }

    void Update()
    {
        if (GamePanel.activeSelf)
        {
            foreach (var cell in _hashGrid)
            {
                cell.Value.Clear();
            }
            Neighbours.Clear();

            calculateHash();
            calNN();
            compDensPress();
            compForce();
            Integrate();
            moveTsunami();
            tsBuildBack();
            TSWBack();
        }
    }

    private void moveTsunami()
    {
        if (TS.Count == 0)
        {
            return;
        }

        if (temperature < 0)
        {
            return;
        }
        
        foreach (var VARIABLE in TS)
        {
            int i = VARIABLE.GetComponent<Particle>().id;
            //Debug.Log(i);
            // for (int j = 0; j < objects.Length; j++)
            // {
                objects[i] = null;
            //}
            //Debug.Log("before "+VARIABLE.GetComponent<Transform>().position);
            if (VARIABLE.GetComponent<Transform>().position.x < 45.7)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(0.25f, 0.0f, 0);
                //Debug.Log("after "+VARIABLE.GetComponent<Transform>().position);
            }
            // Debug.Log(heightStop);
            if (VARIABLE.GetComponent<Transform>().position.y < heightStop)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(0.0f, 0.25f, 0);
            }
        }
    }

    private void tsBuildBack()
    {
        if (TSBuild.Count == 0)
        {
            return;
        }

        if (temperature < 0)
        {
            return;
        }

        foreach (var VARIABLE in TSBuild)
        {
            
            int i = VARIABLE.GetComponent<Particle>().id;
            foreach (var VARIABLE1 in TS.ToList())
            {
                int i1 = VARIABLE1.GetComponent<Particle>().id;
                if (i == i1)
                {
                    TS.Remove(VARIABLE1);
                }
            }

            if (VARIABLE.GetComponent<Transform>().position.x > 23.5)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(-0.05f, 0.0f, 0);
            }

            if (VARIABLE.GetComponent<Transform>().position.y > 11.2)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(0.0f, -0.05f, 0);
            }

        }
    }

    private void TSWBack()
    {
        if (TSWallBack.Count == 0)
        {
            return;
        }

        if (temperature < 0)
        {
            return;
        }

        foreach (var VARIABLE in TSWallBack)
        {
            
            int i = VARIABLE.GetComponent<Particle>().id;
            foreach (var VARIABLE1 in TS.ToList())
            {
                int i1 = VARIABLE1.GetComponent<Particle>().id;
                if (i == i1)
                {
                    TS.Remove(VARIABLE1);
                }
            }

            if (VARIABLE.GetComponent<Transform>().position.x > 40.5)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(-0.05f, 0.0f, 0);
            }

            if (VARIABLE.GetComponent<Transform>().position.y > 11.2)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(0.0f, -0.125f, 0);
            }

        }
    }
    
    private void moveTsunamiRot()
    {
        if (TSRot.Count == 0)
        {
            return;
        }
        foreach (var VARIABLE in TSRot)
        {
            if (VARIABLE.GetComponent<Transform>().position.x < 29 & VARIABLE.GetComponent<Transform>().position.y < 20)
            {
                VARIABLE.GetComponent<Transform>().position += new Vector3(0.5f, 0.5f, 0);
            }
        }
    }
    public int Temperature
    {
        get => temperature;
        set => temperature = value;
    }

    public string Cam
    {
        get => cam;
        set => cam = value;
    }

    private void setCam()
    {
        if (cam == "up")
        {
            upCam.SetActive(true);
            frontCam.SetActive(false);
        }

        if (cam == "front")
        {
            frontCam.SetActive(true);
        }
        if (cam == "left3d")
        {
            left3dCam.SetActive(true);
            frontCam.SetActive(false);
        }
        if (cam == "right3d")
        {
            right3dCam.SetActive(true);
            frontCam.SetActive(false);
        }

    }


    public int Pamount1
    {
        get => Pamount;
        set => Pamount = value;
    }

    public float[] Densities
    {
        get => densities;
        set => densities = value;
    }

    public float[] Pressures
    {
        get => pressures;
        set => pressures = value;
    }

    private void calculateHash()
    {
        int h;
        for (int b = 0; b < objects.Length; b++)
        {
            if(objects[b]==null) continue;
            var cell = hashing.GetCell(objects[b].GetComponent<Transform>().position);
            h = hashing.Hash(cell);
            //Debug.Log(h);
            if (_hashGrid[h].Count == maximumParticlesPerCell)
            {
                //Debug.Log("ffffffff");
                continue;
            }
            _hashGrid[h].Add(b);
        }
    }

    
    
    private void calNN() {
        int f = -1;
        for (int i = 0; i < Cel.Count; i++)
        {

            lab0:
            Vector3Int p1 =(Vector3Int)Cel[i];
            Vector3Int g = p1;
            var h = hashing.Hash(g);
            if (!_hashGrid.ContainsKey(h))
            {
                i = i + 1;
                goto lab0;
            }

            var l = _hashGrid[h];
            if (l.Count == 0)
            {   //Debug.Log(l+".......................");
                continue;
            }
                int a = l[0];
                //N1[a] = new List<int>();
                List<int> ll = new List<int>();
                //Debug.Log(ll+"lllllllllllllllllllllllllllllll");
                //_neighbourTracker[a] = 0;
                Vector3 pos = objects[a].GetComponent<Transform>().position;
                for (int k = 1; k < l.Count; k++)
                {
                    ll.Add(l[k]);
                    //Debug.Log(ll[0]+"22222222222222222");
                }

                Vector3Int g1 = g + new Vector3Int(-1, 0, 0);
                var h1 = hashing.Hash(g1);
                if (!_hashGrid.ContainsKey(h1))
                {
                    goto lab1;
                }

                var l1 = _hashGrid[h1];
                if (l1.Count == 0)
                {
                    goto lab1;
                }

                //int a1 = Random.Range(1, l1.Count);
                int a1 = l1[0];
                //Debug.Log(l1[0]+"   ele 1");
                Vector3 pos1 = objects[a1].GetComponent<Transform>().position;
                Vector3 rij = pos - pos1;
                float r = rij.sqrMagnitude;
                if (r < HSQ)
                {
                    for (int j = 0; j < l1.Count; j++)
                    {
                        ll.Add(l1[j]);
                        //Debug.Log(ll+"    add first element ");
                    }
                }

                lab1:
                //Debug.Log("bey");
                Vector3Int g2 = g + new Vector3Int(1, 0, 0);
                var h2 = hashing.Hash(g2);
                if (!_hashGrid.ContainsKey(h2))
                {
                    goto lab2;
                }

                var l2 = _hashGrid[h2];
                if (l2.Count == 0)
                {
                    goto lab2;
                }

                //int a2 = Random.Range(1, l2.Count);
                int a2 = l2[0];
                Vector3 pos2 = objects[a2].GetComponent<Transform>().position;
                Vector3 rij1 = pos - pos2;
                float r1 = rij1.sqrMagnitude;
                if (r1 < HSQ)
                {
                    for (int j = 0; j < l2.Count; j++)
                    {
                        ll.Add(l2[j]);
                    }
                }

                lab2:
                Vector3Int g3 = g + new Vector3Int(0, 0, -1);
                var h3 = hashing.Hash(g3);
                if (!_hashGrid.ContainsKey(h3))
                {
                    goto lab3;
                }

                var l3 = _hashGrid[h3];
                if (l3.Count == 0)
                {
                    goto lab3;
                }

                //int a3 = Random.Range(1, l3.Count);
                int a3 = l3[0];
                Vector3 pos3 = objects[a3].GetComponent<Transform>().position;
                Vector3 rij2 = pos - pos3;
                float r2 = rij2.sqrMagnitude;
                if (r2 < HSQ)
                {
                    for (int j = 0; j < l3.Count; j++)
                    {
                        ll.Add(l3[j]);
                    }
                }

                lab3:
                Vector3Int g4 = g + new Vector3Int(0, 1, 0);
                var h4 = hashing.Hash(g4);
                if (!_hashGrid.ContainsKey(h4))
                {
                    goto lab4;
                }

                var l4 = _hashGrid[h4];
                if (l4.Count == 0)
                {
                    goto lab4;
                }
                int a4 = l4[0];
                Vector3 pos4 = objects[a4].GetComponent<Transform>().position;
                Vector3 rij3 = pos - pos4;
                float r3 = rij3.sqrMagnitude;
                if (r3 < HSQ)
                {
                    for (int j = 0; j < l4.Count; j++)
                    {
                        ll.Add(l4[j]);
                    }
                }

                lab4:
                Vector3Int g5 = g + new Vector3Int(0, -1, 0);
                var h5 = hashing.Hash(g5);
                if (!_hashGrid.ContainsKey(h5))
                {
                    goto lab5;
                }

                var l5 = _hashGrid[h5];
                if (l5.Count == 0)
                {
                    goto lab5;
                }
                int a5 = l5[0];
                Vector3 pos5 = objects[a5].GetComponent<Transform>().position;
                Vector3 rij4 = pos - pos5;
                float r4 = rij4.sqrMagnitude;
                if (r4 < HSQ)
                {
                    for (int j = 0; j < l5.Count; j++)
                    {
                        ll.Add(l5[j]);
                    }
                }

                lab5:
                Vector3Int g6 = g + new Vector3Int(0, 0, 1);
                var h6 = hashing.Hash(g6);
                if (!_hashGrid.ContainsKey(h5))
                {
                    goto lab7;
                }

                var l6 = _hashGrid[h6];
                if (l6.Count == 0)
                {
                    goto lab7;
                }
                int a6 = l6[0];
                Vector3 pos6 = objects[a6].GetComponent<Transform>().position;
                Vector3 rij5 = pos - pos6;
                float r5 = rij5.sqrMagnitude;
                if (r5 < HSQ)
                {
                    for (int j = 0; j < l6.Count; j++)
                    {
                        ll.Add(l6[j]);
                    }
                }

                lab7:
                //Neighbours[a] = ll;
                //N1[a] = ll;
                if (!Neighbours.ContainsKey(a))
                {
                    Neighbours.Add(a, ll);
                }
                // else
                // {
                    // Debug.Log("elseeeeeeeeeeeeeeeeeeeeeeeeeee");
                    // Neighbours[a] = ll;
                    // Debug.Log("else 22222222222222222222");
                // }
                

                //Debug.Log(a);
                
                for (int u = 1; u < l.Count; u++)
                {   
                    //N1[l[u]].AddRange(ll);
                    //N1[l[u]] = ll;
                    if (!Neighbours.ContainsKey(l[u]))
                    {
                        Neighbours.Add(l[u], ll);
                    }
                    else
                    {
                        continue;
                    }

                    //Debug.Log(l[u]);
                    
                    //Debug.Log(l[u]+"    last");
                    //Debug.Log(N1[l[u]] +"     last2");
                    
                    
                }

            }


        }

    private void InitNeighbourHashing()
    {
        _hashGrid.Clear(); 
        hashing.CellSize = particleRadius *2; // Setting cell-size h to particle diameter.
        hashing.Dimensions = dimensions;
        for (int i = 0; i < dimensions; i++)
        for (int j = 0; j < dimensions; j++)
        for (int k = 0; k < dimensions; k++)
        {
            _hashGrid.Add(hashing.Hash(new Vector3Int(i, j, k)), new List<int>());
            Cel.Add(new Vector3Int(i, j, k));
        }

    }

    void addParticle()
    {
        objects = new GameObject[Pamount];
        densities = new float[Pamount];
        pressures = new float[Pamount];
        forces = new Vector3[Pamount];
        velocities = new Vector3[Pamount];
        int k1 = 0;
        int particlesPerDimension = Mathf.CeilToInt(Mathf.Pow(Pamount, 1f / 3f));
        timer += Time.fixedDeltaTime;
        while (k1 < objects.Length)
        {

            water = Resources.Load("water") as GameObject;

            for (int x = 0; x < particlesPerDimension; x++)
            for (int y = 0; y < particlesPerDimension; y++)
            for (int z = 0; z < particlesPerDimension; z++)
            {
                //Vector3 startPos = new Vector3(dimensions - 1, dimensions - 1, dimensions - 1) -new Vector3(x / 2f, y / 2f, z / 2f) - new Vector3(Random.Range(0f, 0.01f),Random.Range(0f, 0.01f), Random.Range(0f, 0.01f));
                Vector3 startPos = new Vector3(4f, 2f, 0.5f) +new Vector3(x / 2f, y / 2f, z / 2f) + new Vector3(Random.Range(0f, 0.01f),Random.Range(0f, 0.01f), Random.Range(0f, 0.01f));
                w = Instantiate(water, startPos, transform.rotation);
                w.GetComponent<Particle>().x[0] = startPos[0];
                w.GetComponent<Particle>().x[1] = startPos[1];
                w.GetComponent<Particle>().x[2] = startPos[2];
                objects[k1] = w;
                w.GetComponent<Particle>().id = k1;
                // densities[k1] = 0.2f;
                //pressures[k1] = 0.0f;
                forces[k1] = Vector3.zero;
                velocities[k1] = Vector3.zero;
                if (++k1 == Pamount)
                {
                    return;
                }

            }
        }
    }
    
    void compDensPress()
    {
        //Debug.Log(Pamount);
        for (int i = 0; i < objects.Length; i++)
        {
            if(objects[i]==null) continue;
            float sum = 0f;
            densities[i] = Den;
            pressures[i] = pres;
            List<int> t = new List<int>();
            if (Neighbours.ContainsKey(i))
            {
                t = Neighbours[i];
            }
            else
            {
                int hash = hashing.Hash(hashing.GetCell(objects[i].GetComponent<Transform>().position));
                fixing(hash,i);
                t = Neighbours[i];
            }

            foreach (var o in t)
            {
                    int neighbourIndex = o;
                    float distanceSquared =
                        (objects[i].GetComponent<Transform>().position -
                         objects[neighbourIndex].GetComponent<Transform>().position).sqrMagnitude;
                sum += StdKernel(distanceSquared);
            }
            
            densities[i] += sum * MASS + 0.000001f;
                pressures[i] += GAS_CONST * (densities[i] - REST_DENS);
                //Debug.Log(densities[i]);
                //Debug.Log(pressures[i]);

        }
    }

    void fixing(int hash, int index)
    {
        //Debug.Log(index);
        List<int> temp = _hashGrid[hash];
        foreach (var VARIABLE in temp)
        {
            if(VARIABLE==index) continue;
            List<int> temp2 = Neighbours[VARIABLE];
            Neighbours.Add(index ,temp2);
            return;
        }
    }
    void compForce()
    {
        float mass2 = MASS * MASS;
        for(int i=0;i<objects.Length;i++)
        {
            if(objects[i]==null) continue;
            forces[i]=Vector3.zero;
            List<int> t = new List<int>();
            if (Neighbours.ContainsKey(i))
            {
                t = Neighbours[i];
            }
            else
            {
                int hash = hashing.Hash(hashing.GetCell(objects[i].GetComponent<Transform>().position));
                fixing(hash,i);
                t = Neighbours[i];
            }
            var particleDensity2 = densities[i] * densities[i];
            for(int j=0;j<t.Count;j++)
            {
                int neighbourIndex = t[j];

                float distance = ( objects[i].GetComponent<Transform>().position - objects[neighbourIndex].GetComponent<Transform>().position ).magnitude;
                if (distance > 0.0f)
                {
                    var direction = ( objects[i].GetComponent<Transform>().position - objects[neighbourIndex].GetComponent<Transform>().position ) / distance;
                  
                    forces[i] -= mass2 * ( pressures[i] / particleDensity2 + pressures[neighbourIndex] / ( densities[neighbourIndex] * densities[neighbourIndex] ) ) * SpikyKernelGradient(distance, direction);  
                    
                    forces[i] += VISC * mass2 * ( velocities[neighbourIndex] - velocities[i] ) / densities[neighbourIndex] * SpikyKernelSecondDerivative(distance);  
                }
            }
            Vector3 fgrav = G ;
            forces[i] += fgrav;
        }
    }

    float StdKernel(float distanceSquared)
    {
        float x = 1.0f - distanceSquared / radius2;
        return 315f / ( 64f * Mathf.PI * radius3 ) * x * x * x;
    }
    private float SpikyKernelFirstDerivative(float distance)
    {
        float x = 1.0f - distance / particleRadius;
        return -45.0f / ( Mathf.PI * radius4 ) * x * x;
    }
    private float SpikyKernelSecondDerivative(float distance)
    {
        float x = 1.0f - distance / particleRadius;
        return 90f / ( Mathf.PI * radius5 ) * x;
    }
    private Vector3 SpikyKernelGradient(float distance, Vector3 directionFromCenter)
    {
        return SpikyKernelFirstDerivative(distance) * directionFromCenter;
    }
    void Integrate()
    {
        for (int i = 0; i < Pamount; i++)
        {
            // forward Euler integration
            if (temperature <= 0)
            {
                velocities[i] = Vector3.zero;
            }
            else if (temperature > 0 & temperature <= 35)
            {
                velocities[i] += DT * forces[i] / MASS;
            }
            else if (temperature > 35 & temperature <= 60)
            {
                velocities[i] += DT * forces[i] / MASS;
                velocities[i] += 0.004f*velocities[i];
            }
            else if(temperature>60 & temperature<=100)
            {
                velocities[i] += DT * forces[i] / MASS;
                velocities[i] += 0.008f * velocities[i];
            }
            else
            {
                velocities[i] += DT * forces[i] / MASS;
                velocities[i] += 0.016f * velocities[i];
            }
            if(objects[i]==null) continue;
            objects[i].GetComponent<Transform>().position+= DT * velocities[i];
            
            if (objects[i].GetComponent<Transform>().position.x - EPS < 0.0f)
            //if (objects[i].GetComponent<Transform>().position.x - EPS < colliders[1].GetComponent<Transform>().position.x)
            {
                velocities[i].x *= BOUND_DAMPING;
                objects[i].GetComponent<Transform>().position = new Vector3(0.5f,objects[i].GetComponent<Transform>().position.y,
                    objects[i].GetComponent<Transform>().position.z);
            }
            else if(objects[i].GetComponent<Transform>().position.x + EPS > 20) 
            {
                velocities[i].x *= BOUND_DAMPING;
                objects[i].GetComponent<Transform>().position = new Vector3(20-EPS,objects[i].GetComponent<Transform>().position.y,
                    objects[i].GetComponent<Transform>().position.z);
            }
            
            if (objects[i].GetComponent<Transform>().position.y - EPS < 0.0f)
            //if (objects[i].GetComponent<Transform>().position.y - EPS < colliders[0].GetComponent<Transform>().position.y)
            {
                velocities[i].y *= BOUND_DAMPING;
                objects[i].GetComponent<Transform>().position = new Vector3(objects[i].GetComponent<Transform>().position.x
                    ,0.5f,objects[i].GetComponent<Transform>().position.z);
            }
            else if(objects[i].GetComponent<Transform>().position.y + EPS > 10) 
            {
                velocities[i].y *= BOUND_DAMPING;
                objects[i].GetComponent<Transform>().position = new Vector3(objects[i].GetComponent<Transform>().position.x,
                    10-EPS,objects[i].GetComponent<Transform>().position.z);
            }
            
            if (objects[i].GetComponent<Transform>().position.z - EPS < 0.0f) 
            {
                velocities[i].z *= BOUND_DAMPING;
                objects[i].GetComponent<Transform>().position = new Vector3(objects[i].GetComponent<Transform>().position.x,
                    objects[i].GetComponent<Transform>().position.y,  EPS);
            }
            else if(objects[i].GetComponent<Transform>().position.z + EPS > 12) 
            //else if (objects[i].GetComponent<Transform>().position.z + EPS > colliders[2].GetComponent<Transform>().position.z+0.2)
            {
                velocities[i].z *= BOUND_DAMPING;
                objects[i].GetComponent<Transform>().position = new Vector3(objects[i].GetComponent<Transform>().position.x,
                    objects[i].GetComponent<Transform>().position.y, 12-0.5f);
            }
            
            
        }
    }
    
}