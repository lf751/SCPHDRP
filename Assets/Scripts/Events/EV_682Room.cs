using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_682Room : Event_Parent
{
    SmokeController smoke1, smoke2, smoke3;
    public AudioClip scp079;
    public BoxTrigger trigger;

    public override void EventLoad()
    {
        base.EventLoad();
        smoke1 = GameController.instance.getCutsceneObject(x, y, 0).GetComponent<SmokeController>();
        smoke2 = GameController.instance.getCutsceneObject(x, y, 1).GetComponent<SmokeController>();
        smoke3 = GameController.instance.getCutsceneObject(x, y, 2).GetComponent<SmokeController>();
    }

    private void Update()
    {
        if (isStarted)
            EventUpdate();
    }

    public override void EventUpdate()
    {
        base.EventUpdate();
        if(trigger.GetState())
        {
            GameController.instance.GlobalSFX.PlayOneShot(scp079);
            EventFinished();
        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = false;
        smoke1.Switch(true);
        smoke2.Switch(true);
        smoke3.Switch(true);
    }
    /* Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
