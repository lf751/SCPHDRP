using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Tunnel173 : Event_Parent
{
    public BoxTrigger trigger;
    bool isTimer = false;
    float Timer = 2;
    public AudioClip blackOut;
    // Start is called before the first frame update
    public override void EventUpdate()
    {
        base.EventUpdate();
        if(trigger.GetState()&&!isTimer)
        {
            isTimer = true;
            Timer = 2;
            GameController.instance.GlobalSFX.PlayOneShot(blackOut);
            GameController.instance.playercache.FakeBlink(4);
        }
        if(isTimer)
        {
            Timer -= Time.deltaTime;
            if(Timer < 0)
            {
                GameController.instance.npcController.mainList[(int)npc.scp173].Spawn(true, transform.position);
                EventFinished();
            }

        }
    }

    public override void EventFinished()
    {

        base.EventFinished();
        isStarted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
            EventUpdate();
    }
}
