using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Object_Persistent
{
    public CameraPooling controller;
    public Renderer screen;
    public Material turnOff;

    public bool isOn = true;
    public bool is895 = false;
    bool will895 = false;
    public bool canHack = true;
    float timeToHack;
    // Start is called before the first frame update
    public override void Start()
    {
        State = Lib.SetBit(0, 0, isOn);
        State = Lib.SetBit(State, 1, is895);
        float chance = Random.value;
        if (canHack && chance < Scp079Controller.ins.chance895)
        {
            will895 = true;
        }
        else
        {
            will895 = false;
        }
        State = Lib.SetBit(State, 2, will895);

        base.Start();
        Switch(isOn);
        Switch895(is895);
        //GameController.ins.resetState += ResetState;
        if (is895)
            return;

        if (will895)
            timeToHack = Random.Range(Scp079Controller.ins.min895Time, Scp079Controller.ins.max895Time);


    }

    private void Update()
    {
        if (!GameController.ins.isAlive || !GameController.ins.isStart)
            return;

        if (will895 && !is895 && controller.isActiveAndEnabled)
        {
            if (Lib.IsInView(GameController.ins.currPly.PlayerCam, screen.transform) && screen.transform.position.SqrDistance(GameController.ins.currPly.PlayerCam.transform.position) < (Scp079Controller.ins.disToCam * Scp079Controller.ins.disToCam))
            {
                if (timeToHack > 0)
                {
                    timeToHack -= Time.deltaTime;
                    if (timeToHack < 0)
                    {
                        Switch895(true);
                        GameController.ins.GlobalSFX.PlayOneShot(Scp079Controller.ins.HackingSounds[Random.Range(0, Scp079Controller.ins.HackingSounds.Length)]);
                    }
                }
            }
        }
    }

    public override void ResetState()
    {
        base.ResetState();
        //Debug.Log("Loading cam data: " + State + ", is895? " + Lib.GetBit(State, 1));
        isOn = Lib.GetBit(State, 0);
        is895 = Lib.GetBit(State, 1);
        will895 = Lib.GetBit(State, 2);

        timeToHack = Random.Range(Scp079Controller.ins.min895Time, Scp079Controller.ins.max895Time);
    }

    void UpdState()
    {
        State = Lib.SetBit(State, 0, isOn);
        State = Lib.SetBit(State, 1, is895);
        State = Lib.SetBit(State, 2, will895);
        GameController.ins.SetObjectState(State, id);
    }

    public void Switch(bool state)
    {
        //Debug.Log("Switching camera to " + state);
        if (state == false)
        {
            screen.material = turnOff;
            isOn = false;
        }
        else
        {
            isOn = true;
        }
        controller.Switch(state);
        UpdState();
    }

    public void Switch895(bool state)
    {
        //Debug.Log("Switching camera 895 state to " + state);
        is895 = state;
        controller.Switch895(state);
        UpdState();
    }
}
