using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_895 : Event_Parent
{
    public Object_LeverV lever;
    public BoxTrigger trigger;
    public Transform spawn106;
    bool spawned = false;

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
        GameController.ins.globalBools[7] = lever.On;
        //Debug.Log("Is lever?: " + lever.On);

        if (spawned == false && trigger.GetComponent<BoxTrigger>().GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn106.transform.position);
            GameController.ins.npcController.mainList[(int)npc.scp106].transform.rotation = spawn106.transform.rotation;
            spawned = true;
            SetValue(0, 1);
        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        lever.SwitchState(GameController.ins.globalBools[7]);
        spawned = (GetValue(0) == 1);
        isStarted = true;
    }

}
