using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_106_2Way : Event_Parent
{
    public Transform start, decal1, decal2;
    public Transform[] path;
    public Transform[] path2;
    public float Timer, Timer2;
    bool Discover, part2;


    // Update is called once per frame
    void Update()
    {
        if (isStarted == true)
            EventUpdate();
    }

    public override void EventStart()
    {
        if (!isStarted)
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Event_Spawn(true, start.position);
            ((SCP_106)GameController.ins.npcController.mainList[(int)npc.scp106]).SetPath(path);
            Debug.Log("Starting 106 event");
            DecalSystem.instance.Decal(decal1.position, decal1.rotation, 3f, false, 5f, 2, 0);
            isStarted = true;
        }
    }

    public override void EventUpdate()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
        {
            Timer2 -= Time.deltaTime;
            if (!part2)
            {
                ((SCP_106)GameController.ins.npcController.mainList[(int)npc.scp106]).SetPath(path2);
                DecalSystem.instance.Decal(decal2.position, decal2.rotation, 3f, false, 5f, 2, 0);
                part2 = true;
            }
        }

        if (Timer2 <= 0)
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].UnSpawn();
            isStarted = false;
            EventFinished();
        }
        if (Discover)
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].StopEvent();
            isStarted = false;
            EventFinished();
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Discover = true;
    }





}
