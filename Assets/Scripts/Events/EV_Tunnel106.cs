using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Tunnel106 : Event_Parent
{
    public BoxTrigger trigger1, trigger2;
    public Transform spawn1, spawn2;
    public Transform[] spawn173Area1, spawn173Area2;
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
        bool spawned = false;
        if(trigger1.GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn1.position);
            spawned = true;
            EventFinished();
            float rand = Random.value;
            if (rand < 0.25f)
            {
                GameController.ins.npcController.mainList[(int)npc.scp173].Spawn(true, spawn173Area1[Random.Range(0, spawn173Area1.Length)].position);
            }
        }
        if (trigger2.GetState())
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn2.position);
            spawned = true;
            EventFinished();
            float rand = Random.value;
            if (rand < 0.25f)
            {
                GameController.ins.npcController.mainList[(int)npc.scp173].Spawn(true, spawn173Area2[Random.Range(0, spawn173Area2.Length)].position);
            }
        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = false;
    }
}
