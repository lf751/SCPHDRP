using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CameraPooling controller;
    public Renderer screen;
    public Material turnOff;

    public bool isOn = true;
    // Start is called before the first frame update
    void Start()
    {
        Switch(isOn);
    }

    public void Switch(bool state)
    {
        if (state == false)
        {
            screen.material = turnOff;
            controller.enabled = false;
        }
        else
        {
            controller.enabled = true;
        }


    }
}
