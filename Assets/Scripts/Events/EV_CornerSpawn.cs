using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_CornerSpawn : Event_Parent
{ 
    public BoxTrigger side1, side2;
    public Transform corner1, corner2;
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
        if (side1.GetComponent<BoxTrigger>().GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp173].Spawn(true, corner1.transform.position);
            EventFinished();
        }
        if (side2.GetComponent<BoxTrigger>().GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp173].Spawn(true, corner2.transform.position);
            EventFinished();
        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = false;
    }
}
