using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_server096 : Event_Parent
{
    bool outDoorsClosed, inDoorsClosed, isSwitch1, isSwitch2, isSwitch3, shouldBlackOut, isBlackOut;
    int eventState = 0;
    float Timer;
    public BoxTrigger isOut, isIn;
    public Transform doorCloser1, doorCloser2, doorCloser3, doorCloser4, doorCloser5, contraLookAt, step1, step2, scpSpawn, step3, step4, decal1, decal2, decalPuddle, decalPuddle2;
    public Transform [] stepSeq, stepSeq2;
    public GameObject Lights;
    public SCP_096 scp;
    public Object_LeverV leverPump, leverPower, leverGen;
    public LayerMask doorLayer;
    Collider[] Interact;
    public AudioSource audSource, pump, serv, gen;
    public AudioClip scene1, scene2, blackOut;
    public EV_Puppet_Controller guard;
    public ReflectionProbe probe;
    public Material lights;
    public Color32 noLights;
    public float decalScale, decalSplashSpeed;
    Color oldLights;
    // Start is called before the first frame update
    void Awake()
    {
        pump.volume = 0;
        gen.volume = 0;
        serv.volume = 0;

        scp = (SCP_096)GameController.ins.npcController.mainList[(int)npc.scp096];

        decal1.transform.localScale = Vector3.zero;
        decal2.transform.localScale = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            if (GameController.ins.getValue(x, y, 0) == 1)
                DoRoomEvent();
            else
                EventUpdate();
        }
            
        
    }

    public override void EventLoad()
    {
        base.EventLoad();
        lights = GameController.ins.getCutsceneObject(x, y, 0).GetComponent<MeshRenderer>().materials[9];
        oldLights = lights.GetColor("_EmissionColor");
    }

    public override void EventUnLoad()
    {
        base.EventUnLoad();
        GameController.ins.setValue(x, y, 1, leverPump.On ? 1 : 0);
        GameController.ins.setValue(x, y, 2, leverPower.On ? 1 : 0);
        GameController.ins.setValue(x, y, 3, leverGen.On ? 1 : 0);
    }

    public override void EventFinished()
    {
        Debug.Log("Finished played");
        isStarted = true;
        GameController.ins.setDone(x, y);
        if(guard!=null)
            Destroy(guard.gameObject);
        Lights.SetActive(false);
        LockDoors(true);
        isBlackOut = true;
        lights.SetColor("_EmissionColor", noLights);
        GameController.ins.setValue(x, y, 0, 1);

        leverPump.On = GameController.ins.getValue(x, y, 1) == 1;
        leverPower.On = GameController.ins.getValue(x, y, 2) == 1;
        leverGen.On = GameController.ins.getValue(x, y, 3) == 1;

        decal1.transform.localScale = new Vector3(decalScale, decalScale, 1f);
        decal2.transform.localScale = new Vector3(decalScale, decalScale, 1f);
    }

    void LockDoors(bool lockValue)
    {
        Object_Door door;
        Interact = Physics.OverlapSphere(doorCloser1.position, 1f, doorLayer);
        if (Interact.Length > 0)
        {

            door = Interact[0].gameObject.GetComponent<Object_Door>();
            door.isDisabled = lockValue;
        }
        Interact = Physics.OverlapSphere(doorCloser2.position, 1f, doorLayer);
        if (Interact.Length > 0)
        {

            door = Interact[0].gameObject.GetComponent<Object_Door>();
            door.isDisabled = lockValue;
        }
        Interact = Physics.OverlapSphere(doorCloser3.position, 1f, doorLayer);
        if (Interact.Length > 0)
        {

            door = Interact[0].gameObject.GetComponent<Object_Door>();
            door.isDisabled = lockValue;
        }
        Interact = Physics.OverlapSphere(doorCloser4.position, 1f, doorLayer);
        if (Interact.Length > 0)
        {

            door = Interact[0].gameObject.GetComponent<Object_Door>();
            door.isDisabled = lockValue;

        }
        Interact = Physics.OverlapSphere(doorCloser5.position, 1f, doorLayer);
        if (Interact.Length > 0)
        {

            door = Interact[0].gameObject.GetComponent<Object_Door>();
            door.isDisabled = lockValue;
        }
    }

    void DoRoomEvent()
    {
        if (leverPump.On && leverGen.On && leverPower.On)
        {
            shouldBlackOut = false;
        }
        else
            shouldBlackOut = true;

        pump.volume = (leverPump.On == true ? 1 : 0);
        gen.volume = (leverPump.On == true && leverGen.On == true ? 1 : 0);
        serv.volume = (shouldBlackOut == false ? 1 : 0);

        if (shouldBlackOut && !isBlackOut)
        {
            Lights.SetActive(false);
            LockDoors(true);
            audSource.PlayOneShot(blackOut);
            isBlackOut = true;
            GameController.ins.currPly.FakeBlink(0.25f);
            lights.SetColor("_EmissionColor", noLights);
            if(probe!=null)
            probe.RenderProbe();
        }
        if (!shouldBlackOut && isBlackOut)
        {
            Lights.SetActive(true);
            LockDoors(false);
            isBlackOut = false;
            GameController.ins.currPly.FakeBlink(0.25f);
            lights.SetColor("_EmissionColor", oldLights);
            if (probe != null)
                probe.RenderProbe();
        }

        GameController.ins.setValue(x, y, 1, leverPump.On ? 1 : 0);
        GameController.ins.setValue(x, y, 2, leverPower.On ? 1 : 0);
        GameController.ins.setValue(x, y, 3, leverGen.On ? 1 : 0);
    }

    public override void EventStart()
    {
        guard.AnimTrigger(3, true);
        guard.SetRota(contraLookAt);
        scp.Event_Spawn(true, scpSpawn.position);
        scp.transform.rotation = scpSpawn.transform.rotation;
        inDoorsClosed = true;
        isStarted = true;

        GameController.ins.canSave = false;

        GameController.ins.setValue(x, y, 1, 0);
        GameController.ins.setValue(x, y, 2, 0);
        GameController.ins.setValue(x, y, 3, 0);

    }

    public override void EventUpdate()
    {
        if (Time.frameCount % 15 == 0 && eventState != 20 && !outDoorsClosed && isOut.GetState())
        {

            eventState = 1;
            Timer = 2;

            Object_Door door;
            outDoorsClosed = true;
            audSource.clip = scene1;
            SubtitleEngine.instance.playVoice(scene1.name, true);
            audSource.Play();
            Interact = Physics.OverlapSphere(doorCloser1.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;
            }
            Interact = Physics.OverlapSphere(doorCloser2.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;
            }
            Interact = Physics.OverlapSphere(doorCloser3.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;
            }
            Interact = Physics.OverlapSphere(doorCloser4.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                door.isDisabled = false;
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;

            }
            Interact = Physics.OverlapSphere(doorCloser5.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                door.isDisabled = false;
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;
            }
        }

        if (Time.frameCount % 15 == 0 && eventState == 14 && inDoorsClosed)
        {
            /*eventState = 1;
            Timer = 2;*/
            inDoorsClosed = false;
            Object_Door door;
            Interact = Physics.OverlapSphere(doorCloser4.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                door.isDisabled = false;
                if (!door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;

            }
            Interact = Physics.OverlapSphere(doorCloser5.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                door.isDisabled = false;
                if (!door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;
            }
        }

        if (Time.frameCount % 15 == 0 && eventState == 14 && !inDoorsClosed && isIn.GetState())
        {
            eventState = 15;
            inDoorsClosed = true;
            /*
            Timer = 2;*/

            Object_Door door;
            Interact = Physics.OverlapSphere(doorCloser4.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                door.isDisabled = false;
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;
            }
            Interact = Physics.OverlapSphere(doorCloser5.position, 1f, doorLayer);
            if (Interact.Length > 0)
            {

                door = Interact[0].gameObject.GetComponent<Object_Door>();
                door.isDisabled = false;
                if (door.GetState())
                    door.DoorSwitch();
                door.isDisabled = true;

                audSource.PlayOneShot(blackOut);
                GameController.ins.canSave = true;

                GameController.ins.currPly.FakeBlink(0.25f);
                probe.RenderProbe();

                EventFinished();
            }
        }



        Timer -= Time.deltaTime;

        if (Timer < 0)
        {
            switch(eventState)
            {
                case 1:
                    {
                        Debug.Log("Step one");
                        guard.SetPath(new Transform[1] { step1 }, false);
                        eventState = 2;
                        Timer = 3.5f;
                        break;
                    }
                case 2:
                    {
                        guard.AnimTrigger(3, false);
                        eventState = 3;
                        Timer = 1.5f;
                        break;
                    }
                case 3:
                    {
                        Debug.Log("Step two");
                        guard.StopRota();
                        guard.SetPath(stepSeq, false);
                        eventState = 4;
                        Timer = 0.5f;
                        break;
                    }
                case 4:
                    {
                        scp.RotateTo(guard.transform.position);
                        eventState = 5;
                        Timer = 1.5f;
                        break;
                    }
                case 5:
                    {
                        guard.SetLookAt(scp.transform);
                        scp.evChangeState(3);
                        eventState = 6;
                        Timer = 1.5f;
                        break;
                    }
                case 6:
                    {
                        guard.StopLookAt();
                        guard.SetRota(scp.transform);
                        guard.AnimTrigger(1, true);
                        eventState = 7;
                        Timer = 3;
                        break;
                    }
                case 7:
                    {
                        guard.SetPath(new Transform[1] { step3 }, false);
                        eventState = 8;
                        Timer = 3;
                        break;
                    }
                case 8:
                    {
                        guard.SetPath(new Transform[1] { step4 }, false);
                        eventState = 9;
                        Timer = 3;
                        break;
                    }
                case 9:
                    {
                        guard.SetPath(stepSeq2, false);
                        eventState = 10;
                        Timer = 1.5f;
                        break;
                    }
                case 10:
                    {
                        guard.StopRota();
                        guard.AnimTrigger(-3);
                        scp.evWalkTo(stepSeq2[1].position);
                        eventState = 11;
                        Timer = 0.25f;
                        audSource.Stop();
                        audSource.clip = scene2;
                        audSource.Play();
                        break;
                    }
                case 11:
                    {
                        //DecalSystem.instance.Decal(decal1.transform.position, decal1.transform.rotation.eulerAngles, 2, false, 0.2f, 1, 2);
                        //DecalSystem.instance.Decal(decal1.transform.position, decal1.transform.rotation.eulerAngles-new Vector3(0,180,0), 2, false, 0.2f, 1, 2);

                        Tween.LocalScale(decal1, new Vector3(decalScale, decalScale, 1f), decalSplashSpeed, 0f, Tween.EaseOutStrong);
                        Tween.LocalScale(decal2, new Vector3(decalScale, decalScale, 1f), decalSplashSpeed, 0f, Tween.EaseOutStrong);
                        DecalSystem.instance.Decal(decalPuddle.position, decalPuddle.rotation, 5f, false, 1f, 1, 2);
                        DecalSystem.instance.Decal(decalPuddle2.position, decalPuddle2.rotation, 4f, false, 1f, 1, 0);

                        eventState = 12;
                        Timer = 0.25f;
                        break;
                    }
                case 12:
                    {
                        eventState = 13;
                        //scp.evChangeState(0);
                        Timer = 10f;
                        Destroy(guard.gameObject);
                        break;
                    }
                case 13:
                    {
                        eventState = 14;
                        scp.StopEvent();
                        Debug.Log("096 Stopped");
                        break;
                    }
            }
        }

    }
}
