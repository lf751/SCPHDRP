using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_EmilyDeath : Event_Parent
{
    public AudioClip Scream;
    public AudioSource Source;
    public Transform pos;
    // Start is called before the first frame update
    void Start()
    {
        if (GameController.ins.getValue(x,y,0)==0)
        {
            Source.PlayOneShot(Scream);
            GameController.ins.setValue(x, y, 0, 1);
        }
    }

    public override void EventStart()
    {
        base.EventStart();
        GameController.ins.GlobalSFX.PlayOneShot(Scp079Controller.ins.FlickSounds[Random.Range(0, Scp079Controller.ins.FlickSounds.Length)]);
        GameController.ins.particleController.StartParticle(0, pos.transform.position, pos.transform.rotation);
        if (GameController.ins.getValue(x, y, 1) == 0)
        {
            GameController.ins.getCutsceneObject(x, y, 0).GetComponent<Object_Door>().DoorSwitch();
            GameController.ins.setValue(x, y, 1, 1);
        }
        EventFinished();
    }
}
