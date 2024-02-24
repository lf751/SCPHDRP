using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Tunnel106 : Event_Parent
{
    public BoxTrigger trigger1, trigger2;
    public Transform spawn1, spawn2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
            EventUpdate();
    }

    public override void EventUpdate()
    {
        base.EventUpdate();
        if(trigger1.GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn1.position);
            EventFinished();
        }
        if (trigger2.GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn2.position);
            EventFinished();
        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = false;
    }
}
