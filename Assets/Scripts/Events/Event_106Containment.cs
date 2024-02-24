using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event_106Containment : Event_Parent
{

    public Object_LeverV leverMag, leverAud;
    public Object_Button_Trigger buttonBreak;

    public Box106 box;

    CameraController cameraCont;

    public AudioClip[] lureIdle;
    public AudioClip magUp, magDown, femurBreak, attack106, cry;
    public AudioSource paSystem, paSystem2, boxSound;

    public Animator lureSubject;

    public Transform spawn1061, spawn1062, decal1, decal2;

    public float audMax, audMin, spawnMin, spawnMax, decalSize1, decalSize2;

    float currTimer, audioTimer;

    public override void EventLoad()
    {
        base.EventLoad();
        cameraCont = GameController.ins.getCutsceneObject(x, y, 0).GetComponent<CameraController>();
        decal2 = GameController.ins.getCutsceneObject(x, y, 1).transform;
        state = GameController.ins.getValue(x, y, 0);
        currTimer = GameController.ins.getValue(x, y, 1);
        
        leverMag.On = GameController.ins.getValue(x, y, 2) == 1;
        Debug.Log("magnet should be: " + leverMag.On);
        leverAud.On = GameController.ins.getValue(x, y, 3) == 1;

        decal1.transform.localScale = Vector3.zero;
        decal2.transform.localScale = Vector3.zero;
    }

    public override void EventFinished()
    {
        base.EventFinished();
        if (state == 6)
            cameraCont.Switch(false);
        isStarted = false;
    }

    public override void EventUpdate()
    {
        base.EventUpdate();

        currTimer -= Time.deltaTime;

        audioTimer -= Time.deltaTime;
        if (audioTimer < 0)
        {
            if (state == 1)
            {
                paSystem2.clip = lureIdle[Random.Range(0, lureIdle.Length-1)];
                paSystem2.Play();
                audioTimer = paSystem2.clip.length + Random.Range(audMin, audMax);
            }
            /*if(state == 4)
            {
                paSystem2.clip = cry;
                paSystem2.Play();
                audioTimer = cry.length + Random.Range(audMin, audMax);
            }*/
        }

        
        if(state==1&&buttonBreak.activated)
        {
            state = 2;
            paSystem2.Stop();
            paSystem2.clip = femurBreak;
            paSystem2.Play();
            currTimer = 3.4f;
        }

        if (currTimer < 0)
        {
            switch(state)
            {
                case 2:
                    {
                        if (leverAud.On)
                        {
                            currTimer = femurBreak.length - 3.3f;
                            lureSubject.SetTrigger("toPain");
                            state = 3;
                        }
                        else
                            EventFinished();
                        break;
                    }
                case 3:
                    {
                        currTimer = Random.Range(spawnMin, spawnMax);
                        state = 4;
                        break;
                    }
                case 4:
                    {
                        if(!leverMag.On)
                        {
                            GameController.ins.npcController.mainList[(int)npc.scp106].transform.rotation = spawn1061.rotation;
                            GameController.ins.npcController.mainList[(int)npc.scp106].Event_Spawn(false, spawn1061.position);
                            Tween.LocalScale(decal1, new Vector3(decalSize1, decalSize1, 1f), 4f, 0f, Tween.EaseOutStrong);
 

                            currTimer = 7;
                            state = 5;
                            paSystem2.Stop();
                            paSystem2.clip = attack106;
                            paSystem2.Play();
                        }
                        else
                        {
                            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn1062.position);
                            EventFinished();
                        }
                        break;
                    }
                case 5:
                    {
                        Tween.LocalScale(decal2, new Vector3(decalSize2, decalSize2, 1f), 3f, 0f, Tween.EaseOutStrong);
                        currTimer = 3;
                        state = 6;
                        break;
                    }
                case 6:
                    {
                        if (leverMag.On)
                        {
                            GameController.ins.npcController.mainList[(int)npc.scp106].UnSpawn();
                            GameController.ins.npcController.mainList[(int)npc.scp106].data.state = npcstate.death;
                            EventFinished();
                        }
                        else
                        {
                            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn1062.position);
                            EventFinished();
                        }
                        break;
                    }


            }
        }


        GameController.ins.setValue(x, y, 0, state);
        GameController.ins.setValue(x, y, 1, (int)currTimer);
    }

    private void Update()
    {
        if (isStarted)
            EventUpdate();

        AlwaysUpdate();
    }

    void AlwaysUpdate()
    {
        if (leverMag.On && !box.isFloating)
        {
            box.isFloating = true;
            boxSound.PlayOneShot(magUp);
            GameController.ins.setValue(x, y, 2, (leverMag.On ? 1:0));
        }
        if (!leverMag.On && box.isFloating)
        {
            box.isFloating = false;
            boxSound.PlayOneShot(magDown);
            GameController.ins.setValue(x, y, 2, (leverMag.On ? 1 : 0));
        }

        if(leverAud.On == paSystem.mute)
        {
            paSystem.mute = !leverAud.On;
            paSystem2.mute = !leverAud.On;
            GameController.ins.setValue(x, y, 3, (leverAud.On ? 1 : 0));
        }

        if ((state == 2 || state == 3 || state == 4)&& !isStarted)
        {
            audioTimer -= Time.deltaTime;
            if (audioTimer < 0)
            {
                paSystem2.clip = cry;
                paSystem2.Play();
                audioTimer = cry.length + Random.Range(audMin, audMax);
            }
        }

        if (!isStarted && GameController.ins.npcController.mainList[(int)npc.scp106].data.state == npcstate.death && !leverMag.On)
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].data.state = npcstate.alive;
            GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, spawn1062.position);
        }
    }



    public override void EventStart()
    {
        base.EventStart();
        if (state==0)
        {
            Debug.Log("Setup 106");
            leverMag.SwitchState(true);
            GameController.ins.setValue(x, y, 2, 1);
            state = 1;
        }
        if(state==2)
        {
            paSystem2.Stop();
            paSystem2.clip = attack106;
            paSystem2.time = currTimer;
            paSystem2.Play();
        }
        if (state == 3)
        {
            paSystem2.Stop();
            paSystem2.clip = attack106;
            paSystem2.time = 3.4f+currTimer;
            paSystem2.Play();
        }
        if(state == 4)
        {
            GameController.ins.npcController.mainList[(int)npc.scp106].Event_Spawn(false, spawn1061.position);
        }
    }


    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
}
