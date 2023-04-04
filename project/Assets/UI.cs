using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public GameObject UIPanel;
    public GameObject GamePanel;
    public GameObject ReEnter;
    private string cam;
    private int temper;
    private float press;
    private float dens;
    private int Pam;
    private float x;
    private float y;
    private float z;
    private float x2 = float.PositiveInfinity;
    private float y2 = float.PositiveInfinity;
    private float z2 = float.PositiveInfinity;
    public GameObject Button;
    public manager Manage;
    

    public void tempChanged(string newstr)
    {
        temper = int.Parse(newstr);
    }
    public void PamChanged(string newstr)
    {
        Pam = int.Parse(newstr);
    }
    public void PressChanged(string newstr)
    {
        press = float.Parse(newstr);
    }
    public void ViscChanged(string newstr)
    {
        dens = float.Parse(newstr);
    }
    public void XChanged(string newstr)
    {
        x = float.Parse(newstr);
    }
    public void YChanged(string newstr)
    {
        y = float.Parse(newstr);
    }
    public void ZChanged(string newstr)
    {
        z = float.Parse(newstr);
    }
    public void CamChanged(string newstr)
    {
        cam = newstr;
    }
    public void X2Changed(string newstr)
    {
        x2 = float.Parse(newstr);
    }
    public void Y2Changed(string newstr)
    {
        y2 = float.Parse(newstr);
    }
    public void Z2Changed(string newstr)
    {
        z2 = float.Parse(newstr);
    }
    
    private void setValues()
    {
        Manage.Temperature = temper;
        Manage.Pamount1 = Pam;
        manager.Den = dens;
        manager.pres = press;
        Manage.Cam = cam;
        Manage.TsX = x;
        Manage.TsY = y;
        Manage.TsZ = z;
        if (x2 != float.PositiveInfinity & y2 != float.PositiveInfinity & z2 != float.PositiveInfinity)
        {
            if (x2 >= 0 & x2 <= 17 & y2 >= 1 & y2 <= 9 & z2 >= 0 & z2 <= 10)
            {
                manager.x2 = x2;
                manager.y2 = y2;
                manager.z2 = z2;
            }
        }
    }
    
    public void X()
    {
        ReEnter.SetActive(false);
        UIPanel.SetActive(true);
    }
    
    public void click()
    {
        if (x >= 0 & x <= 17 & y >= 1 & y <= 9 & z >= 0 & z <= 10 & Pam<=3000)
        {
            setValues();
            UIPanel.SetActive(false);
            GamePanel.SetActive(true);
        }
        else
        {
            UIPanel.SetActive(false);
            ReEnter.SetActive(true);
        }
        /*Debug.Log(Pam);
        Debug.Log(temper);
        Debug.Log(press);
        Debug.Log(visc);
        Debug.Log(x);
        Debug.Log(y);
        Debug.Log(z);*/
    }
}
