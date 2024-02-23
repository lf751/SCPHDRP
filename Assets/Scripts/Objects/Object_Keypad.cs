using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Keypad : Object_Captive
{
    public GameObject Door01, Door02;
    public string code;
    public AudioClip Accepted, Rejected;
    public AudioSource soundsource;
    public bool fromGlobal = false;
    public int globalIndex;

    private void Start()
    {
        if(fromGlobal)
        {
            code = GameController.instance.globalStrings[globalIndex];
        }
    }

    public override void StartCapture()
    {
        SCP_UI.instance.ToggleKeypad(this);
    }

    public override void EndCaptive()
    {
        base.EndCaptive();
        SCP_UI.instance.ToggleKeypad(this);
    }

    public void checkCode(string attempt)
    {
        if (code == attempt)
        {
            Door01.GetComponent<Object_Door>().DoorSwitch();
            if (Door02 != null)
                Door02.GetComponent<Object_Door>().DoorSwitch();
            soundsource.PlayOneShot(Accepted);
            SubtitleEngine.instance.playSub("playStrings", "play_button_code");
        }
        else
        {
            SubtitleEngine.instance.playSub("playStrings", "play_button_wrongcode");
            soundsource.PlayOneShot(Rejected);
        }
    }



}
