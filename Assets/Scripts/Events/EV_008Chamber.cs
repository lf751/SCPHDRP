using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_008Chamber : Event_Parent
{

    public SmokeController gameSmoke;
    public GameObject glass, spawn173, crash173, scrambleArea;
    public AudioClip glassCrash;
    Object_Door doorChamber, doorRoom;
    public Object_LeverV lever;
    public BoxTrigger airLock, chamber;
    public float maxTime, minTime;

    float Timer;

    public bool isActive=true;
    public ParticleSystem gas,smoke;
    bool isEmmiting = true, isTimerActive = false, spawned = false;
    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    public override void EventLoad()
    {
        base.EventLoad();
        Debug.Log("Getting Cutscene Object");
        doorChamber = GameController.instance.getCutsceneObject(x, y, 0).GetComponent<Object_Door>();
        doorRoom = GameController.instance.getCutsceneObject(x, y, 1).GetComponent<Object_Door>();
        lever.On = GameController.instance.globalBools[1];
    }

    public override void EventUpdate()
    {
        base.EventUpdate();

        if (!spawned && airLock.GetState() && doorChamber.switchOpen)
        {
            GameController.instance.npcController.mainList[(int)npc.scp173].Event_Spawn(true, spawn173.transform.position);
            spawned = true;
        }

        if (spawned && !isTimerActive && chamber.GetState())
        {
            Timer = Random.Range(minTime, maxTime);
            isTimerActive = true;
        }

        if (isTimerActive)
        {
            //Debug.Log("Is chamber = " + chamber.GetState());
            Timer -= Time.deltaTime;
            if(Timer < 0)
            {
                if (GameController.instance.playercache.IsBlinking())
                {
                    //Debug.Log("Finishing");
                    GameController.instance.playercache.FakeBlink(0.3f);
                    GameController.instance.npcController.mainList[(int)npc.scp173].Event_Spawn(true, crash173.transform.position);
                    GameController.instance.PlayHorror(glassCrash, crash173.transform, npc.none);

                    if(chamber.GetState() && ((GameController.instance.playercache.equipment[(int)bodyPart.Head]==null) ? true : (ItemController.instance.items[GameController.instance.playercache.equipment[(int)bodyPart.Head].itemFileName].itemName!="hazmat")))
                    {
                        Debug.Log("Zombie now");
                        GameController.instance.playercache.hasZombie = true;
                        SubtitleEngine.instance.playSub("playStrings", "play_zombie_cut");
                    }

                    EventFinished();
                }
            }
        }

    }


    // Update is called once per frame
    void Update()
    {
        scrambleArea.SetActive(!doorRoom.GetState());

        isActive = !lever.On;
        GameController.instance.globalBools[1] = lever.On;

        if (isActive!=isEmmiting)
        {
            var emission = gas.emission;
            emission.enabled = isActive;

            emission = smoke.emission;
            emission.enabled = isActive;

            isEmmiting = isActive;

            gameSmoke.Switch(isActive);
        }

        if (isStarted)
            EventUpdate();

    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = false;
        glass.SetActive(false);
    }
}
