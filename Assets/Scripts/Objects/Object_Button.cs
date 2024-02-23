using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Object_Button : Object_Interact
{
    public AudioClip click;
    public AudioSource soundsource;

    public GameObject Door01, Door02;
    public override void Pressed()
    {
            Door01.GetComponent<Object_Door>().DoorSwitch();
            if (Door02 != null)
                Door02.GetComponent<Object_Door>().DoorSwitch();
        soundsource.PlayOneShot(click);
    }
    
    public override void Hold()
    {
    }
}
