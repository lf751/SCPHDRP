using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Spawn096 : Event_Parent
{
    // Start is called before the first frame update
    public override void EventLoad()
    {
        if (GameController.ins.getValue(x, y, 0) == 0)
        {
            GameController.ins.npcController.mainList[(int)npc.scp096].Spawn(true, transform.position);
            EventFinished();
            SetValue(0, 1);
        }

        
    }

}
