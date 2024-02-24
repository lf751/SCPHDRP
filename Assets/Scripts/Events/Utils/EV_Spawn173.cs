using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Spawn173 : Event_Parent
{
    // Start is called before the first frame update
    public override void EventStart()
    {
        base.EventStart();
        GameController.ins.npcController.mainList[(int)npc.scp173].Spawn(true, transform.position);

        EventFinished();
    }

}
