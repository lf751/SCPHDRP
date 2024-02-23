using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Intro : MonoBehaviour
{
    public Transform[] Path1;
    public AudioClip[] Dialogs, RefuseDiag, DI_Escort, DI_Angry1, DI_Angry2, DI_Done1, DI_Done2, FinalEmpty, ConverA, ConverB, Ambiance;
    public AudioClip Gunshot, MusIntro;
    public Transform talktome, playerPos;
    public GameObject Guard1, Guard2, Door1, Door2, Door3, Trigger1, Trigger2, Trigger3, Trigger4, Gas1, Gas2, NextScene, Item, introZone;
    GameObject Player;
    Transform playerHead;
    public Transform leftTurn;
    EV_Puppet_Controller Guard1_con, Guard2_con;
    IEnumerator co;

    int EventState = 0;
    int EventState2 = 0;
    int LastState = -1;
    int Conver = -1;

    bool introEventStart = false;

    public float Timer1, Timer1_5, Timer2, Timer3, Timer, TimerSecondary, LastTimer,Refuse1,Refuse2,Refuse3, RunawayTimer;
    bool StopTimer = false, StopTimer2 = false, ActiveTimer = true, ActiveTimer2 = true, Check1 = false, Check2 = false, Check3 = false, Check4 = false, runningAway, back1 = false, Start = false, grabbed = false;

    private void Awake()
    {
        GameController.instance.startGame.AddListener(StartEvent);
    }
    private void OnDestroy()
    {
        GameController.instance.startGame.RemoveListener(StartEvent);
    }

    void StartEvent()
    {
        if (GlobalValues.playIntro)
        {
            GameController.instance.ambianceController.ChangeAmbiance(Ambiance, 8);
            MusicPlayer.instance.StartMusic(MusIntro);
            GameController.instance.StopTimer = false;
            GameController.instance.doGameplay = false;
            GameController.instance.playercache.playerWarp(playerPos.transform.position, playerPos.transform.eulerAngles.y);
            introEventStart = true;
        }
        else
        {
            //
            RenderSettings.fog = true;
            GameController.instance.DefMusic();
            GameController.instance.DefaultAmbiance();
            GameController.instance.StopTimer = true;
            GameController.instance.doGameplay = true;
            GameController.instance.canSave = true;
            GameController.instance.CullerFlag = true;
            Destroy(introZone);
            if (!SaveSystem.instance.playData.worldsCreateds[(int)GameController.instance.worldName])
                GameController.instance.SetMapPos(0, 10);
            DestroyImmediate(this);
        }

        Guard1_con = Guard1.GetComponent<EV_Puppet_Controller>();
        Guard2_con = Guard2.GetComponent<EV_Puppet_Controller>();
        playerHead = Camera.main.transform;
        Player = GameController.instance.player;
        Timer = 5;

        if (GlobalValues.mapseed.Contains("IntroConvo"))
        {
            AsyncScene_1();
            Door1.GetComponent<Object_Door>().DoorSwitch();
            ActiveTimer = false;
        }
    }

    private void Update()
    {
        if (introEventStart)
            IntroUpdate();
    }

    void IntroUpdate()
    {
        if (ActiveTimer)
            Timer -= Time.deltaTime;
        if (runningAway)
            RunawayTimer -= Time.deltaTime;

        if (RunawayTimer <= 0.0f && runningAway == true)
        {
            if (EventState == -5)
                Escape5();
            if (EventState == -4)
                Escape4();
            if (EventState == -3)
                Escape3();
            if (EventState == -2)
                Escape2();
            if (EventState == -1)
                Escape1();
        }

        if (Timer <= 0.0f && Start == false)
        {

            Timer = Timer1;
            Guard1_con.PlaySound(Dialogs[0],true);

            if (grabbed == false)
                SCP_UI.instance.ShowTutorial("tutograb");
            Start = true;
        }

        if (grabbed == false && Item == null)
        {
            SCP_UI.instance.ShowTutorial("tutoinv1");
            grabbed = true;
        }







        if (Timer <= 0.0f && StopTimer == false && Start == true)
        {

            if (EventState == -13)
                EVRefuse3();
            if (EventState == -12)
                EVRefuse2();
            if (EventState == -11)
                EVRefuse1();


            if (EventState == 3)
            {
                Scene3();
                StopTimer = true;
            }
            if (EventState == 2)
            {
                Scene2();
            }
            if (EventState == 1)
                Scene1_5();

            if (EventState == 0)
                Scene1();
        }






        if (ActiveTimer2)
            TimerSecondary -= Time.deltaTime;
        if (TimerSecondary <= 0.0f && StopTimer2 == false)
        {

            if (EventState2 == 2)
            {
                AsyncScene_2();
            }
            if (EventState2 == 1)
            {
                AsyncScene_1();
            }
        }






        if (Check1 == true)
        {
            if (Trigger1.GetComponent<BoxTrigger>().GetState())
            {
                Check1 = false;
                ActiveTimer = true;
                Scene1_6();
            }
        }

        if (Check3 == true)
        {
            if (Trigger3.GetComponent<BoxTrigger>().GetState())
            {
                if (!back1)
                {
                    Guard1_con.PlaySound(Dialogs[5],true);
                    back1 = true;
                }
                Guard1_con.ResumePath();
                Guard1_con.StopRota();

                Guard1_con.AnimTrigger(1, false);

                Guard1_con.StopLookAt();
                Guard2_con.ResumePath();

                LastState = EventState;

                Check3 = false;
                Check2 = true;
                ActiveTimer = true;
                runningAway = false;
                EventState = 3;
            }
        }

        if (Check2 == true)
        {
            if (!Trigger2.GetComponent<BoxTrigger>().GetState() && EventState != -1)
            {
                Guard1_con.StopSound();
                Guard2_con.StopSound();
                EventState = LastState;
                if (!back1)
                {   
                    RunawayTimer = 6;
                    Guard1_con.PlaySound(Dialogs[2],true, true);
                }

                if (Conver != -1)
                {
                    StopCoroutine(co);
                    Conver = -1;
                }

                Guard1_con.SetLookAt(playerHead);
                ActiveTimer = false;
                ActiveTimer2 = false;
                runningAway = true;

                Guard1_con.SetRota(Player.transform);
                Guard2_con.SetLookAt(playerHead);
                Guard2_con.PausePath();
                Guard1_con.PausePath();
                
                Check2 = false;
                Check3 = true;
            }
        }

        if (Check4 == true)
        {
            if (Trigger4.GetComponent<BoxTrigger>().GetState())
            {
                Guard1_con.StopPursuit();
                Guard1_con.PlaySound(Gunshot);
                Player.GetComponent<Player_Control>().Death(0);
                GameController.instance.deathmsg = Localization.GetString("deathStrings","death_intro");
                Check4 = false;
                Check3 = false;
                Check2 = false;
                runningAway = false;
                Guard1_con.AnimTrigger(-1, true);
            }
        }


    }

    void Scene1()
    {
        Timer = Timer1_5;
        Door1.GetComponent<Object_Door>().DoorSwitch();
        Guard1_con.SetLookAt(playerHead);
        Guard1_con.PlaySound(Dialogs[1],true);
        EventState = 1;
        Guard2_con.AnimTrigger(2);


    }

    void Scene1_5()
    {
        Check1 = true;
        EventState = -11;
        Timer = Refuse1;
    }

    void EVRefuse1()
    {
        Guard1_con.PlaySound(RefuseDiag[Random.Range(0, 2)],true);
        EventState = -12;
        Timer = Refuse2;
    }
    void EVRefuse2()
    {
        Guard1_con.PlaySound(RefuseDiag[2],true);
        EventState = -13;
        Timer = Refuse3;
    }
    void EVRefuse3()
    {
        Door1.GetComponent<Object_Door>().DoorSwitch();
        Gas1.SetActive(true);
        Gas2.SetActive(true);
        StopTimer = true;
    }


    void Scene1_6()
    {
        Guard1_con.PlaySound(DI_Escort[Random.Range(0, DI_Escort.Length)],true, true);
        Timer = Timer2;
        EventState = 2;
    }

    void Scene2()
    {
        Guard1_con.SetRota(leftTurn, true);
        Guard1_con.SetPath(Path1, false);
        Guard1_con.StopLookAt();
        Guard2_con.SetRota(leftTurn, true);
        Guard2_con.SetPath(Path1, false);
        //Guard2_con.SetLookAt(playerHead);
        EventState = 3;
        Timer = Timer3;

        ActiveTimer2 = true;
        TimerSecondary = 5;
        EventState2 = 1;


        Check2 = true;
    }

    void Scene3()
    {
        if (back1)
        {
            FinalEmpty[1] = DI_Done2[Random.Range(0, DI_Done2.Length)];
            FinalEmpty[0] = DI_Done1[Random.Range(1, DI_Done1.Length)];
            Guard1_con.SetSeq(FinalEmpty, true);
        }
        else
            Guard1_con.PlaySound(DI_Done1[0], true);

        Door3.GetComponent<Object_Door>().DoorSwitch();
        Guard1_con.SetLookAt(playerHead);
        Guard2_con.StopLookAt();
        Guard2_con.PausePath();
        
        Check2 = false;
        NextScene.SetActive(true);

    }

    public void End()
    {
        Guard2_con.StopLookAt();
        Guard1_con.StopLookAt();
        Guard1_con.StopRota();
        Guard2_con.StopRota();
    }





    void AsyncScene_1()
    {
        Conver = Random.Range(0, ConverA.Length);

        switch(GlobalValues.mapseed)
        {
            case "IntroConvo1":
                {
                    Conver = 0;
                    break;
                }
            case "IntroConvo2":
                {
                    Conver = 1;
                    break;
                }
            case "IntroConvo3":
                {
                    Conver = 2;
                    break;
                }
            case "IntroConvo4":
                {
                    Conver = 3;
                    break;
                }
            case "IntroConvo5":
                {
                    Conver = 4;
                    break;
                }
        }


        Debug.Log(Conver);



        Guard2_con.PlaySound(ConverB[Conver]);
        Guard1_con.PlaySound(ConverA[Conver]);


        if (Conver == 0)
        {
            co = Convo1();
        }
        if (Conver == 1)
        {
            co = Convo2();
        }
        if (Conver == 2)
        {
           co =Convo3();
        }
        if (Conver == 3)
        {
            co = Convo4();
        }
        if (Conver == 4)
        {
            co = Convo5();
        }

        StartCoroutine(co);
        Guard1_con.SetLookAt(talktome);
        EventState2 = 2;
        TimerSecondary = 4.5f;
    }

    IEnumerator Convo1()
    {
        SubtitleEngine.instance.playVoice("Intro_Convo1_1");
        yield return new WaitForSeconds(3);
    }

    IEnumerator Convo2()
    {
        SubtitleEngine.instance.playVoice("Intro_Convo2_1");
        yield return new WaitForSeconds(3);
        }

    IEnumerator Convo3()
    {
        SubtitleEngine.instance.playVoice("Intro_Convo3_1");
        yield return new WaitForSeconds(4);
    }

    IEnumerator Convo4()
    {
        SubtitleEngine.instance.playVoice("Intro_Convo4_1"); 
        yield return new WaitForSeconds(1);
    }

    IEnumerator Convo5()
    {
        SubtitleEngine.instance.playVoice("Intro_Convo5_1");
        yield return new WaitForSeconds(2);
    }






    void AsyncScene_2()
    {
        Guard1_con.StopLookAt();
        StopTimer2 = true;
    }


    void Escape1()
    {
        Guard1_con.StopRota();
        Guard1_con.SetLookAt(playerHead);
        Guard1_con.PlaySound(Dialogs[3],true, true);
        EventState = -2;
        RunawayTimer = 5;
    }

    void Escape2()
    {
        Guard1_con.PlaySound(Dialogs[4],true, true);
        EventState = -3;
        RunawayTimer = 5;
    }

    void Escape3()
    {
        Guard1_con.PlaySound(DI_Angry1[Random.Range(0, DI_Angry1.Length)],true, true);
        Guard1_con.AnimTrigger(1,true);
        EventState = -4;
        RunawayTimer = 12;
    }

    void Escape4()
    {
        Guard1_con.PlaySound(DI_Angry2[Random.Range(0, DI_Angry2.Length)],true, true);
        Guard1_con.AnimTrigger(1, true);
        EventState = -5;
        RunawayTimer = 5;
    }

    void Escape5()
    {
        Guard1_con.SetPursuit(Player.transform);
        Guard1_con.AnimTrigger(1, true);
        runningAway = false;
        Check3 = false;
        Check4 = true;
    }


}
