using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_EmilyDeath : Event_Parent
{
    public AudioClip Scream, voice079;
    public AudioSource Source;
    public Transform pos;
    // Start is called before the first frame update
    void Start()
    {
        if (GameController.instance.getValue(x,y,0)==0)
        {
            Source.PlayOneShot(Scream);
            GameController.instance.setValue(x, y, 0, 1);
        }
    }

    public override void EventStart()
    {
        base.EventStart();
        GameController.instance.GlobalSFX.PlayOneShot(voice079);
        GameController.instance.particleController.StartParticle(0, pos.transform.position, pos.transform.rotation);
        if (GameController.instance.getValue(x, y, 1) == 0)
        {
            GameController.instance.getCutsceneObject(x, y, 0).GetComponent<Object_Door>().DoorSwitch();
            GameController.instance.setValue(x, y, 1, 1);
        }
        EventFinished();
    }
}
