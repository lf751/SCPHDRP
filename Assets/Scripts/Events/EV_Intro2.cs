using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Intro2 : MonoBehaviour
{
    public GameObject door1, door2, tri1, tri2, tri3, lightmana, emelight, d1, d2, sci, sci2, guard, flask, RerenderProbe1, RerenderProbe2, introZone;
    public Transform[] path1, path2, path3, path4, path5;
    public Transform ata1, ata2, ata3, ataFinal, TeleportAnchor, LookLeft, LookRight;
    GameObject objPlayer;
    public EV_Intro firstEvent;
    EV_Puppet_Controller d1_, d2_, sci_, guard_, sci2_;
    public AudioClip[] Dialogs, Refuse, NewAmbiance, Horror, GeneralSFX, Alarm, guardExit, guardWhat;
    public AudioClip MusicChange, classd1, classd2, guardDies;
    int eventstat = 0, refusestat = 0;
    float Timer, TimerSecondary=0.1f, refuseTimer;
    public float Timer1, Timer2, Timer3, Timer4, Timer5, refuseTimer1, refuseTimer2, refuseTimer3, refuseTimer4;
    bool ActiveTimer, StopTimer, check2, check3, StopTimer2, ActiveTimer2, ActiveRefuse;
    // Start is called before the first frame update
    void OnEnable()
    {
        objPlayer = GameController.instance.player;
        d1_ = d1.GetComponent<EV_Puppet_Controller>();
        d2_ = d2.GetComponent<EV_Puppet_Controller>();
        sci_ = sci.GetComponent<EV_Puppet_Controller>();
        sci2_ = sci2.GetComponent<EV_Puppet_Controller>();
        guard_ = guard.GetComponent<EV_Puppet_Controller>();

        GameController.instance.ChangeMusic(MusicChange);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (eventstat == 0)
        {
            d2_.AnimTrigger(2, true);
            if (tri1.GetComponent<BoxTrigger>().GetState())
            {
                eventstat = 1;
                Debug.Log("Door 2 switch");
                door2.GetComponent<Object_Door>().DoorSwitch();
                d1_.SetLookAt(Camera.main.transform);
                Timer = Timer1;
                ActiveTimer = true;

            }
        }

        if (check2 == true)
        {
            if (tri2.GetComponent<BoxTrigger>().GetState())
            {
                Debug.Log("Closing door");
                door1.GetComponent<Object_Door>().DoorSwitch();
                sci_.PlaySound(Dialogs[1], true);
                sci_.AnimTrigger(-6);
                check2 = false;
                Timer = 4f;
                eventstat = 5;
                ActiveTimer = true;
                ActiveRefuse = false;
                StopTimer = false;
                guard_.StopRota();
                d1_.StopLookAt();
                d2_.StopLookAt();
            }
        }

        if (check3 == true)
        {
            if (tri3.GetComponent<BoxTrigger>().GetState())
            {
                guard_.SetLookAt(objPlayer.transform);
                guard_.PlaySound(guardExit[Random.Range(0, guardExit.Length)]);
                check3 = false;
            }
        }




        if (ActiveTimer)
            Timer -= Time.deltaTime;
        if (ActiveRefuse)
        {
            refuseTimer -= Time.deltaTime;

            if (refuseTimer <= 0)
            {
                switch(refusestat)
                {
                    case 0:
                        {
                            sci_.PlaySound(Refuse[0], true);
                            refusestat = 1;
                            refuseTimer = refuseTimer2;
                            guard_.SetRota(objPlayer.transform);
                            d2_.SetLookAt(GameController.instance.playercache.DefHead);
                            break;
                        }
                    case 1:
                        {
                            refusestat = 2;
                            sci_.PlaySound(Refuse[1], true);
                            refuseTimer = refuseTimer3;
                            break;
                        }
                    case 2:
                        {
                            d1_.StopLookAt();
                            d2_.StopLookAt();
                            refusestat = 3;
                            guard_.AnimTrigger(1, true);
                            sci_.PlaySound(Refuse[2], true);
                            Debug.Log("Another door opening, cause refuse");
                            door1.GetComponent<Object_Door>().DoorSwitch();
                            refuseTimer = refuseTimer4;
                            break;
                        }
                    case 3:
                        {
                            guard_.PlaySFX(GeneralSFX[4]);
                            GameController.instance.playercache.Death(0);
                            ActiveRefuse = false;
                            break;
                        }

                }


            }
        }

        if (Timer <= 0.0f && StopTimer == false)
        {
            switch (eventstat)
            {
                case 1:
                    {
                        d1_.StopLookAt();
                        d2_.StopLookAt();
                        Debug.Log("Door should open");
                        door1.GetComponent<Object_Door>().DoorSwitch();
                        sci_.PlaySound(Dialogs[0],true);
                        sci_.AnimTrigger(-6);
                        Timer = Timer2;
                        eventstat = 2;
                        ActiveRefuse = true;
                        refuseTimer = refuseTimer1;
                        break;
                    }

                case 2:
                    {
                        d1_.SetPath(path1);
                        GameController.instance.PlayHorror(Horror[0], ata1, npc.none);
                        eventstat = 3;
                        Timer = 0.8f;
                        break;
                    }

                case 3:
                    {
                        d2_.SetPath(path2);
                        eventstat = 4;
                        Timer = Timer3;
                        break;
                    }

                case 4:
                    {
                        check2 = true;
                        StopTimer = true;
                        d1_.SetLookAt(GameController.instance.npcController.mainList[(int)npc.scp173].transform);
                        d2_.SetLookAt(GameController.instance.npcController.mainList[(int)npc.scp173].transform);
                        break;
                    }

                case 5:
                    {
                        d1_.SetPath(path3);
                        StartCoroutine(WaitForWalk());
                        eventstat = 6;
                        Timer = 3f;
                        break;
                    }
                case 6:
                    {
                        Debug.Log("Door should open on chaos");
                        door1.GetComponent<Object_Door>().DoorSwitch();
                        check3 = true;
                        sci_.PlaySound(Dialogs[2], true);
                        sci_.AnimTrigger(-6);
                        d2_.SetRota(door1.transform);
                        d2_.StopLookAt();
                        StartCoroutine(SmallSequence());
                        eventstat = 7;
                        Timer = Timer4;
                        break;

                    }
                case 7:
                    {
                        objPlayer.GetComponent<Player_Control>().FakeBlink(0.6f);
                        lightmana.SetActive(false);
                        d1_.PlaySound(classd2);
                        eventstat = 8;
                        Timer = 0.7f;
                        d1_.AnimTrigger(-1, true);

                        GameController.instance.npcController.mainList[(int)npc.scp173].transform.rotation = Quaternion.Euler(0, 90, 0);
                        GameController.instance.npcController.mainList[(int)npc.scp173].Spawn(false, ata1.position);
                        DecalSystem.instance.Decal(ata1.transform.position, new Vector3(90f, 0, 0), 2f, true, 0.4f, 1, 2);
                        d1_.StopLookAt();
                        
                        break;
                    }
                case 8:
                    {
                        d2_.StopLookAt();
                        d2_.AnimTrigger(-5);
                        d1_.PlaySFX(GeneralSFX[3]);
                        lightmana.SetActive(true);
                        sci2_.AnimTrigger(-4);
                        sci_.PlaySFX(GeneralSFX[0]);
                        eventstat = 9;
                        Timer = 1f;
                        GameController.instance.PlayHorror(Horror[1], null, npc.none);
                        break;
                    }
                case 9:
                    {
                        lightmana.SetActive(false);
                        objPlayer.GetComponent<Player_Control>().FakeBlink(0.3f);
                        eventstat = 10;
                        Timer = 0.5f;
                        GameController.instance.npcController.mainList[(int)npc.scp173].Spawn(false, ata2.position);
                        DecalSystem.instance.Decal(ata2.transform.position, new Vector3(90f, 0, 0), 4f, true, 0.4f, 1, 0);
                        d2_.AnimTrigger(-2, true);
                        d2_.StopLookAt();
                        d2_.PlaySFX(GeneralSFX[3]);
                        break;
                    }
                case 10:
                    {
                        check3 = false;
                        GameController.instance.npcController.mainList[(int)npc.scp173].Event_Spawn(true, ata2.transform.position);
                        guard_.PlaySound(guardWhat[Random.Range(0, guardWhat.Length)]);

                        sci_.PlaySFX(GeneralSFX[1]);
                        lightmana.SetActive(true);
                        sci2_.AnimTrigger(1, true);
                        Timer = Timer5;
                        eventstat = 11;
                        break;
                    }
                case 11:
                    {
                        lightmana.SetActive(false);
                        objPlayer.GetComponent<Player_Control>().FakeBlink(0.4f);
                        eventstat = 12;
                        Timer = 0.7f;
                        GameController.instance.npcController.mainList[(int)npc.scp173].transform.rotation = Quaternion.Euler(0, -90, 0);
                        GameController.instance.npcController.mainList[(int)npc.scp173].Spawn(false, ata3.position);
                        guard_.StopLookAt();
                        guard_.PlaySound(guardDies);
                        guard_.SetRota(GameController.instance.npcController.mainList[(int)npc.scp173].transform);
                        sci_.PlaySFX(GeneralSFX[5]);
                        break;
                    }

                case 12:
                    {
                        sci_.PlaySFX(GeneralSFX[2]);
                        emelight.SetActive(true);
                        guard_.AnimTrigger(1, true);
                        Timer = 1.5f;
                        eventstat = 13;
                        ActiveTimer2 = true;
                        if (GetComponent<ReflectionProbe>() != null)
                        {
                            RerenderProbe1.GetComponent<ReflectionProbe>().RenderProbe();
                            RerenderProbe2.GetComponent<ReflectionProbe>().RenderProbe();
                        }
                        break;
                    }
                case 13:
                    {
                        emelight.SetActive(false);
                        objPlayer.GetComponent<Player_Control>().FakeBlink(0.4f);
                        GameController.instance.npcController.mainList[(int)npc.scp173].Spawn(false, ataFinal.position);
                        eventstat = 14;
                        Timer = 0.7f;
                        guard_.AnimTrigger(-2, true);
                        
                        guard_.PlaySFX(GeneralSFX[3]);
                        sci_.PlaySFX(GeneralSFX[5]);
                        sci_.PlaySFX(GeneralSFX[6]);
                        StopTimer2 = true;
                        flask.SetActive(false);
                        break;
                    }

                case 14:
                    {
                        objPlayer.GetComponent<Player_Control>().FakeBlink(0.6f);
                        sci_.SetSeq(Alarm);

                        guard_.StopRota();
                        objPlayer.GetComponent<Player_Control>().playerWarp((GameController.instance.WorldAnchor.transform.position + ((GameController.instance.WorldAnchor.transform.rotation * Quaternion.Inverse(TeleportAnchor.transform.rotation)) * (objPlayer.transform.position - TeleportAnchor.position))), GameController.instance.WorldAnchor.transform.eulerAngles.y - TeleportAnchor.transform.eulerAngles.y);
                        d1_.puppetWarp(GameController.instance.WorldAnchor.transform.position + ((GameController.instance.WorldAnchor.transform.rotation * Quaternion.Inverse(TeleportAnchor.transform.rotation)) * (d1.transform.position - TeleportAnchor.position)), GameController.instance.WorldAnchor.transform.eulerAngles.y - TeleportAnchor.transform.eulerAngles.y);
                        d2_.puppetWarp(GameController.instance.WorldAnchor.transform.position + ((GameController.instance.WorldAnchor.transform.rotation * Quaternion.Inverse(TeleportAnchor.transform.rotation)) * (d2.transform.position - TeleportAnchor.position)), GameController.instance.WorldAnchor.transform.eulerAngles.y - TeleportAnchor.transform.eulerAngles.y);
                        guard_.puppetWarp(GameController.instance.WorldAnchor.transform.position + ((GameController.instance.WorldAnchor.transform.rotation * Quaternion.Inverse(TeleportAnchor.transform.rotation)) * (guard.transform.position - TeleportAnchor.position)), GameController.instance.WorldAnchor.transform.eulerAngles.y - TeleportAnchor.transform.eulerAngles.y);
                        

                        d1_.DeactivateCollision();
                        d2_.DeactivateCollision();

                        StopTimer = true;

   
                        
                        firstEvent.End();

                        if (GameController.instance.isAlive)
                        {
                            GameController.instance.DefaultAmbiance();
                            GameController.instance.DefMusic();
                            GameController.instance.doGameplay = true;
                            GameController.instance.Action_QuickSave();
                            GameController.instance.SetMapPos(0, 10);
                            GameController.instance.canSave = true;
                            RenderSettings.fog = true;
                            GameController.instance.CullerFlag = true;
                            Destroy(introZone);
                        }
                        break;
                    }

            }


        }






        if (ActiveTimer2)
            TimerSecondary -= Time.deltaTime;
        if (TimerSecondary <= 0.0f && StopTimer2 == false)
        {
            if (flask.activeSelf == true)
                flask.SetActive(false);
            else
                flask.SetActive(true);

            guard_.PlaySFX(GeneralSFX[4]);
            TimerSecondary = 0.1f;
        }




    }


    IEnumerator SmallSequence()
    {
        yield return new WaitForSeconds(4f);
        d2_.SetPath(path4);
        yield return new WaitForSeconds(3f);
        d2_.SetLookAt(LookLeft);
        d1_.AnimTrigger(1, true);
        yield return new WaitForSeconds(1f);
        d2_.PlaySound(classd1);
        yield return new WaitForSeconds(1.5f);
        d2_.SetLookAt(LookRight);
    }

    IEnumerator WaitForWalk()
    {
        yield return new WaitForSeconds(1.3f);
        d2_.SetPath(path5);
    }




}
