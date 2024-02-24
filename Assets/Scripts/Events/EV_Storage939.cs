using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Storage939 : Event_Parent
{
    public BoxTrigger trigger1;
    public Object_LeverV lever1, lever2;
    public Object_Door door;
    public GameObject audio;
    public Transform scp1, scp2, scp3;
    public Transform[] path1, path2, path3;
    public AudioClip[] hello1, hello2, hello3, heard1, heard2, heard3, found1, found2, found3, attack1, attack2, attack3;

    bool active_lev1, active_lev2, up_ele1=true, up_ele2=true;

    public override void EventLoad()
    {
        Debug.Log("Loading StorageEvent");
        base.EventLoad();
        /*if (isStarted)
        {
            int firstSCP = GameController.instance.getValue(x, y, 3);

            ((NPC_939)GameController.instance.npcController.NPCS[firstSCP]).NpcEnable();
            ((NPC_939)GameController.instance.npcController.NPCS[firstSCP+1]).NpcEnable();
            ((NPC_939)GameController.instance.npcController.NPCS[firstSCP+2]).NpcEnable();
        }*/
    }

    public override void EventUnLoad()
    {
        Debug.Log("Unloading StorageEvent");
        base.EventLoad();
        if (isStarted)
        {
            
            int firstSCP = GameController.ins.getValue(x, y, 3);
            Debug.Log("Dynamic NPC number = " + firstSCP);

            ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).NpcDisable();
            ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 1]).NpcDisable();
            ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 2]).NpcDisable();
        }
    }

    // Start is called before the first frame update
    public override void EventStart()
    {
        base.EventStart();
        GameController.ins.setValue(x, y, 3, GameController.ins.npcController.AddNpc(npctype.scp939, scp1.transform.position));
        GameController.ins.npcController.AddNpc(npctype.scp939, scp2.transform.position);
        GameController.ins.npcController.AddNpc(npctype.scp939, scp3.transform.position);


        EventFinished();
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger1.GetComponent<BoxTrigger>().GetState())
        {
            audio.SetActive(true);
            if (GameController.ins.getValue(x, y, 2) == 0)
            {
                SCP_UI.instance.ShowTutorial("tutohide2");
                GameController.ins.setValue(x, y, 2, 1);
            }
        }
        else
        {
            //Debug.Log("Audio modifier de-activated");
            audio.SetActive(false);
        }

        if (active_lev1 != lever1.On)
        {
            GameController.ins.setValue(x, y, 0, lever1.On ? 1 : 0);
            if (lever1.On || lever2.On)
            {
                if (!door.switchOpen)
                    door.DoorSwitch();
            }
            else if (door.switchOpen)
                door.DoorSwitch();

            active_lev1 = lever1.On;
        }

        if (active_lev2 != lever2.On)
        {
            GameController.ins.setValue(x, y, 1, lever2.On ? 1 : 0);
            if (lever1.On || lever2.On)
            {
                if (!door.switchOpen)
                    door.DoorSwitch();
            }
            else if (door.switchOpen)
                door.DoorSwitch();

            active_lev2 = lever2.On;
        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = true;
        active_lev1 = (GameController.ins.getValue(x, y, 0) == 1);
        active_lev2 = (GameController.ins.getValue(x, y, 1) == 1);

        lever1.On = active_lev1;
        lever2.On = active_lev2;

        if (lever1.On || lever2.On)
        {
            if (!door.switchOpen)
                door.DoorSwitch();
        }

        int firstSCP = GameController.ins.getValue(x, y, 3);
        Debug.Log("Dynamic NPC number loaded = " + firstSCP);

        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).patrol = path1;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).Hello = hello1;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).Heard = heard1;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).Found = found1;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).Attack = attack1;


        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP+1]).patrol = path2;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 1]).Hello = hello2;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 1]).Heard = heard2;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 1]).Found = found2;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 1]).Attack = attack2;


        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP+2]).patrol = path3;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 2]).Hello = hello3;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 2]).Heard = heard3;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 2]).Found = found3;
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 2]).Attack = attack3;

        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP]).NpcEnable();
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 1]).NpcEnable();
        ((NPC_939)GameController.ins.npcController.NPCS[firstSCP + 2]).NpcEnable();
    }
}
