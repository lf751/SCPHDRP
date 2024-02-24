using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_ElevJanitor : Event_Parent
{
    public Transform decal1, decal2;
    public EV_Puppet_Controller janitor;

    public override void EventLoad()
    {
        base.EventLoad();
        if (GameController.ins.getValue(x, y, 0) == 0)
        {
            GameController.ins.getCutsceneObject(x, y, 0).GetComponent<Object_Door>().DoorSwitch();
            GameController.ins.setValue(x, y, 0, 1);
        }
    }

    public override void EventStart()
    {
        if (!isStarted)
        {
            EventFinished();
            isStarted = true;
        }
    }
    public override void EventFinished()
    {
        DecalSystem.instance.Decal(decal1.position, decal1.rotation, 2.5f, true, 3.5f, 1, 2);
        DecalSystem.instance.Decal(decal2.position, decal2.rotation, 2f, true, 2f, 0, 2);
        janitor.AnimTrigger(-8, true);
        base.EventFinished();
    }
}
