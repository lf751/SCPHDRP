using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Chamber049 : Event_Parent
{
    bool isDone = false, blackoutSet = false, isBlackout = false, eventSet = false, isWakeUpZombies=false, isSpawn049=false, isMusic=false;
    float Timer;
    public float shutDownTimer;
    public BoxTrigger isDown, trigger1, trigger2;
    public Transform spawn1, spawn2, zombie1, zombie2, zombie3;
    public GameObject lights;
    public AudioSource audSource, fuelPump;
    public AudioClip blackOut, chamberMusic;
    public Object_Elevator elev1, elev2;
    public Object_LeverV energyPump, elev;
    public Material lightMat;
    public Color32 noLights;
    Color oldLights;

    // Start is called before the first frame update
    void Start()
    {
        elev1 = GameController.instance.getCutsceneObject(x, y, 0).GetComponent<Object_Elevator>();
        elev2 = GameController.instance.getCutsceneObject(x, y, 1).GetComponent<Object_Elevator>();

        lightMat = GameController.instance.getCutsceneObject(x, y, 2).GetComponent<MeshRenderer>().materials[10];
        oldLights = lightMat.GetColor("_EmissionColor");

    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            if (!eventSet)
            {
                EventUpdate();
            }
            else
            {
                EventLast();
            }
        }
    }

    public override void EventStart()
    {
        base.EventStart();
        GameController.instance.setValue(x, y, 2, energyPump.On ? 1 : 0);
        GameController.instance.setValue(x, y, 3, elev.On ? 1 : 0);
    }

    public override void EventUpdate()
    {
        if (!blackoutSet && isDown.GetState())
        {
            Timer = shutDownTimer;
            blackoutSet = true;
            GameController.instance.ChangeMusic(chamberMusic);
            elev1.isDisabled = true;
            elev2.isDisabled = true;
            isMusic = true;
        }

        Timer -= Time.deltaTime;

        if (blackoutSet && !isBlackout && Timer < 0)
        {
            lightMat.SetColor("_EmissionColor", noLights);
            isBlackout = true;
            GameController.instance.playercache.FakeBlink(0.25f);
            audSource.PlayOneShot(blackOut);
            lights.SetActive(false);
            EventFinished();

            int firstID = GameController.instance.npcController.AddNpc(npctype.zombie, zombie1.transform.position);

            GameController.instance.setValue(x, y, 0, firstID);
            GameController.instance.npcController.AddNpc(npctype.zombie, zombie2.transform.position);
            GameController.instance.npcController.AddNpc(npctype.zombie, zombie3.transform.position);

            GameController.instance.npcController.NPCS[firstID].NpcDisable();
            GameController.instance.npcController.NPCS[firstID+1].NpcDisable();
            GameController.instance.npcController.NPCS[firstID+2].NpcDisable();

        }
    }

    public override void EventFinished()
    {
        base.EventFinished();
        eventSet = true;

        energyPump.On = GameController.instance.getValue(x, y, 2) == 1;
        elev.On = GameController.instance.getValue(x, y, 3) == 1;
    }

    public override void EventUnLoad()
    {
        base.EventUnLoad();
        GameController.instance.setValue(x, y, 2, energyPump.On ? 1 : 0);
        GameController.instance.setValue(x, y, 3, elev.On ? 1 : 0);
    }

    void EventLast()
    {
        if(isDown.GetState())
        {
            if (!isMusic)
            {
                GameController.instance.ChangeMusic(chamberMusic);
                isMusic = true;
            }
        }
        else
        {
            if (isMusic)
            {
                Debug.Log("ApagandoMusica");
                MusicPlayer.instance.StopMusic();
                //GameController.instance.DefMusic();
                isMusic = false;
            }
        }

        fuelPump.volume = (energyPump.On == true ? 1 : 0);

        blackoutSet = !energyPump.On;

        if (blackoutSet && !isBlackout)
        {
            lightMat.SetColor("_EmissionColor", noLights);
            lights.SetActive(false);
            audSource.PlayOneShot(blackOut);
            isBlackout = true;
            GameController.instance.playercache.FakeBlink(0.25f);
        }
        if (!blackoutSet && isBlackout)
        {
            lightMat.SetColor("_EmissionColor", oldLights);
            lights.SetActive(true);
            isBlackout = false;
            GameController.instance.playercache.FakeBlink(0.25f);
            if (GameController.instance.getValue(x, y, 1) != 1)
            {
                int firstID = GameController.instance.getValue(x, y, 0);
                GameController.instance.setValue(x, y, 1, 1);

                GameController.instance.npcController.NPCS[firstID].NpcEnable();
                GameController.instance.npcController.NPCS[firstID + 1].NpcEnable();
                GameController.instance.npcController.NPCS[firstID + 2].NpcEnable();
            }
        }

        if (energyPump.On && elev.On)
        {
            elev1.isDisabled = false;
            elev2.isDisabled = false;

            if (!isSpawn049 && trigger1.GetState())
            {
                elev1.FloorUp = false;
                elev2.FloorUp = false;

                elev1.OpenDoors();
                elev2.OpenDoors();

                isSpawn049 = true;
                GameController.instance.npcController.mainList[(int)npc.scp049].Spawn(true, spawn1.position);
                ((NPC_049)GameController.instance.npcController.mainList[(int)npc.scp049]).allowMapPath = false;
            }
            if (!isSpawn049 && trigger2.GetState())
            {
                elev1.FloorUp = false;
                elev2.FloorUp = false;

                elev1.OpenDoors();

                elev2.OpenDoors();
                isSpawn049 = true;
                GameController.instance.npcController.mainList[(int)npc.scp049].Spawn(true, spawn2.position);
                ((NPC_049)GameController.instance.npcController.mainList[(int)npc.scp049]).allowMapPath = false;
            }
        }
    }

}
