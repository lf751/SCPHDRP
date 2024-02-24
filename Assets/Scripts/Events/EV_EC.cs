using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_EC : Event_Parent
{
    public Object_LeverV leverDoor;
    public Object_LeverV leverLights;
    public bool currLightsValue;
    public AudioClip audLightSwitch;

    /*public override void EventLoad()
    {
        base.EventLoad();
        Debug.Log("Getting Cutscene Object");
        lever.On = GameController.ins.globalBools[7];
    }*/

    public override void EventStart()
    {
        base.EventStart();
        EventFinished();
    }

    void Update()
    {
        if (isStarted)
        {
            EventUpdate();
        }
    }

    public override void EventUpdate()
    {
        GameController.ins.globalBools[6] = leverDoor.On;
        GameController.ins.globalBools[3] = leverLights.On;

        if (GameController.ins.globalBools[3] != currLightsValue)
        {
            GameController.ins.lightsOn = GameController.ins.globalBools[3];
            GameController.ins.UpdateLights();

            if (GameController.ins.lightsOn)
                GameController.ins.GlobalSFX.PlayOneShot(audLightSwitch);
        }
        currLightsValue = GameController.ins.globalBools[3];
    }

    public override void EventFinished()
    {
        base.EventFinished();
        leverDoor.SwitchState(GameController.ins.globalBools[6]);
        leverLights.SwitchState(GameController.ins.globalBools[3]);
        isStarted = true;
        currLightsValue = GameController.ins.globalBools[3];
    }

}
