using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_SCP012 : Event_Parent
{
    public Object_Door door;
    public AudioSource golgotha;
    public AudioClip [] Dvoice;
    public MeshRenderer Pages;
    public Texture [] bloodPages;
    public Transform[] Path;
    public Transform blood;
    public BoxTrigger trigger, scp012;
    public Animator box;
    public float StartTimer = 90;
    float Timer;
    bool check=true, check2=true, check3 = true, shesamaniac = false, audio1=false, audio2=false, audio3=false, audio4=false, audio5=false, audio6=false, audio7=false;


    // Start is called before the first frame update
    void Start()
    {
        
    }
    public override void EventStart()
    {
        base.EventStart();
        if (GameController.instance.getValue(x,y,0)== 0)
        {
            SCP_UI.instance.ShowTutorial("tutoinv3");
            GameController.instance.setValue(x, y, 0, 1);

        }
    }

    public override void EventUpdate()
    {
        if (trigger.GetComponent<BoxTrigger>().GetState())
        {
            if (check == true)
            {
                EventFinished();
            }
            if (check2 == true)
            {
                GameController.instance.player.GetComponent<Player_Control>().ForceWalk(Path);
                check2 = false;
            }

        }
        else
        {
            if (check2 == false)
            {
                check2 = true;
                GameController.instance.player.GetComponent<Player_Control>().StopWalk();
            }
        }

        if (scp012.GetComponent<BoxTrigger>().GetState() && check3 == true)
        {
            Timer -= Time.deltaTime;

            if (audio1 == false)
            {
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[0]);
                SubtitleEngine.instance.playVoice("scene_012_1");
                audio1 = true;
            }

            if (audio2 == false && Timer < StartTimer-12)
            {
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[1]);
                SubtitleEngine.instance.playVoice("scene_012_2");
                SubtitleEngine.instance.playSub("playStrings", "play_012_1");
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x + 0.3f, blood.transform.position.y, blood.transform.position.z - 0.15f), new Vector3(90f, 0f, 0f), 1.0f, false, 0.5f, 0, 1);
                audio2 = true;
                

            }
            if (audio3 == false && Timer < StartTimer - 30)
            {
                GameController.instance.playercache.bloodloss += 1;
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[2]);
                SubtitleEngine.instance.playVoice("scene_012_3");
                SubtitleEngine.instance.playSub("playStrings", "play_012_2");
                audio3 = true;
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x - 0.1f, blood.transform.position.y, blood.transform.position.z + 0.25f), new Vector3(90f, 0f, 0f), 2.0f, false, 0.5f, 0, 0);
                Pages.material.mainTexture = bloodPages[0];
            }

            if (audio4 == false && Timer < StartTimer - 48)
            {
                SubtitleEngine.instance.playSub("playStrings", "play_012_3");
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[3]);
                SubtitleEngine.instance.playVoice("scene_012_4");
                Pages.material.mainTexture = bloodPages[1];
                audio4 = true;
            }
            if (audio5 == false && Timer < StartTimer - 60)
            {
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[4]);
                GameController.instance.playercache.bloodloss = 2;
                SubtitleEngine.instance.playVoice("scene_012_5");
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x + 0.1f, blood.transform.position.y, blood.transform.position.z - 0.15f), new Vector3(90f, 0f, 0f), 2.0f, false, 0.5f, 0, 1);
                audio5 = true;
            }

            if (audio6 == false && Timer < StartTimer - 74)
            {
                GameController.instance.playercache.bloodloss += 1;
                SubtitleEngine.instance.playSub("playStrings", "play_012_4");
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[5]);
                SubtitleEngine.instance.playVoice("scene_012_6");
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x + 0.2f, blood.transform.position.y, blood.transform.position.z - 0.1f), new Vector3(90f, 0f, 0f), 2.0f, false, 0.5f, 0, 2);
                audio6 = true;
                Pages.material.mainTexture = bloodPages[2];
            }
            if (audio7 == false && Timer < StartTimer - 86)
            {
                GameController.instance.GlobalSFX.PlayOneShot(Dvoice[6]);
                SubtitleEngine.instance.playVoice("scene_012_7");
                audio7 = true;
            }

            if (Timer <= 0)
            {
                GameController.instance.deathmsg = Localization.GetString("deathStrings", "death_012");
                GameController.instance.player.GetComponent<Player_Control>().Death(0);
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x - 0.03f, blood.transform.position.y, blood.transform.position.z + 0.02f), new Vector3(90f, 0f, 0f), 3.0f, false, 3.0f, 1, 2);
                check3 = false;
            }
            if (shesamaniac == false)
            {
                GameController.instance.playercache.CognitoHazard(true);
                shesamaniac = true;
            }
        }
        else
        {
            if (shesamaniac == true)
            {
                GameController.instance.playercache.CognitoHazard(false);
                shesamaniac = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted == true)
            EventUpdate();
    }

    public override void EventFinished()
    {
        box.SetBool("start", true);
        golgotha.Play();
        door.DoorSwitch();

        check = false;
        isStarted = true;
        Timer = StartTimer;

        base.EventFinished();

    }
}
