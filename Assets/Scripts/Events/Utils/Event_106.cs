using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_106 : Event_Parent
{
    // Start is called before the first frame update
    public GameObject trigger, spawnHere;
    bool spawned = false;

    private void Update()
    {
        if (isStarted == true)
            EventUpdate();
    }

    // Update is called once per frame
    public override void EventUpdate()
    {
        if (spawned == false && trigger.GetComponent<BoxTrigger>().GetState())
        {
            GameController.instance.npcController.mainList[(int)npc.scp106].Spawn(true, spawnHere.transform.position);
            spawned = true;
            EventFinished();
        }
    }
}
