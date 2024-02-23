using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Tunnel_Gas : Event_Parent
{
    public BoxTrigger trigger;
    public SmokeController cont;
    public AudioClip gasLeak;
    // Start is called before the first frame update
    public override void EventUpdate()
    {
        base.EventUpdate();
        if (trigger.GetState())
        {
            EventFinished();
        }
    }

    public override void EventFinished()
    {
        GameController.instance.GlobalSFX.PlayOneShot(gasLeak);
        cont.Switch(true);
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
