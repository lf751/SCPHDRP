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
        if (GameController.ins.getValue(x,y,0)== 0)
        {
            SCP_UI.instance.ShowTutorial("tutoinv3");
            GameController.ins.setValue(x, y, 0, 1);
        }
    }

    public override void EventUpdate()
    {
        bool cognProt=false;

        for (int i = 0; i< 4; i++)
        {
            GameItem currItem = GameController.ins.currPly.equipment[i];
            if (currItem == null)
                continue;
            Item itemData = ItemController.instance.items[currItem.itemFileName];
            if (itemData is Equipable_Wear && ((Equipable_Wear) itemData).protectCogn)
            {
                cognProt = true;
            }
        }


        if (trigger.GetComponent<BoxTrigger>().GetState())
        {
            if (check == true)
            {
                EventFinished();
            }
            if (!cognProt)
            {
                if (check2 == true)
                {
                    GameController.ins.currPly.ForceWalk(Path);
                    //shesamaniac = false;
                    check2 = false;
                }
            }
        }
        else
        {
            if (check2 == false)
            {
                check2 = true;
                GameController.ins.currPly.StopWalk();
                shesamaniac = false;
            }
        }

        if(cognProt)
        {
            if (check2 == false)
            {
                check2 = true;
                GameController.ins.currPly.StopWalk();
            }
        }

        

        if (scp012.GetComponent<BoxTrigger>().GetState() && check3 == true && !cognProt)
        {
            Timer -= Time.deltaTime;

            if (audio1 == false)
            {
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[0]);
                SubtitleEngine.instance.playVoice("scene_012_1");
                audio1 = true;
            }

            if (audio2 == false && Timer < StartTimer-12)
            {
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[1]);
                SubtitleEngine.instance.playVoice("scene_012_2");
                SubtitleEngine.instance.playSub("playStrings", "play_012_1");
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x + 0.3f, blood.transform.position.y, blood.transform.position.z - 0.15f), Quaternion.identity, 1.0f, false, 0.5f, 0, 1);
                audio2 = true;
                

            }
            if (audio3 == false && Timer < StartTimer - 30)
            {
                GameController.ins.currPly.bloodloss += 1;
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[2]);
                SubtitleEngine.instance.playVoice("scene_012_3");
                SubtitleEngine.instance.playSub("playStrings", "play_012_2");
                audio3 = true;
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x - 0.1f, blood.transform.position.y, blood.transform.position.z + 0.25f), Quaternion.identity, 2.0f, false, 0.5f, 0, 0);
                Pages.material.mainTexture = bloodPages[0];
            }

            if (audio4 == false && Timer < StartTimer - 48)
            {
                SubtitleEngine.instance.playSub("playStrings", "play_012_3");
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[3]);
                SubtitleEngine.instance.playVoice("scene_012_4");
                Pages.material.mainTexture = bloodPages[1];
                audio4 = true;
            }
            if (audio5 == false && Timer < StartTimer - 60)
            {
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[4]);
                GameController.ins.currPly.bloodloss = 2;
                SubtitleEngine.instance.playVoice("scene_012_5");
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x + 0.1f, blood.transform.position.y, blood.transform.position.z - 0.15f), Quaternion.identity, 2.0f, false, 0.5f, 0, 1);
                audio5 = true;
            }

            if (audio6 == false && Timer < StartTimer - 74)
            {
                GameController.ins.currPly.bloodloss += 1;
                SubtitleEngine.instance.playSub("playStrings", "play_012_4");
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[5]);
                SubtitleEngine.instance.playVoice("scene_012_6");
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x + 0.2f, blood.transform.position.y, blood.transform.position.z - 0.1f), Quaternion.identity, 2.0f, false, 0.5f, 0, 2);
                audio6 = true;
                Pages.material.mainTexture = bloodPages[2];
            }
            if (audio7 == false && Timer < StartTimer - 86)
            {
                GameController.ins.GlobalSFX.PlayOneShot(Dvoice[6]);
                SubtitleEngine.instance.playVoice("scene_012_7");
                audio7 = true;
            }

            if (Timer <= 0)
            {
                GameController.ins.deathmsg = Localization.GetString("deathStrings", "death_012");
                GameController.ins.player.GetComponent<PlayerControl>().Death(0);
                DecalSystem.instance.Decal(new Vector3(blood.transform.position.x - 0.03f, blood.transform.position.y, blood.transform.position.z + 0.02f), Quaternion.identity, 3.0f, false, 3.0f, 1, 2);
                check3 = false;
            }
            if (shesamaniac == false)
            {
                GameController.ins.currPly.CognitoHazard(true);
                GameController.ins.currPly.ForceLook(scp012.transform.position, 3f);
                Debug.Log("Turning on cognitoeffect");
                shesamaniac = true;
            }
        }
        else
        {
            if (shesamaniac == true)
            {
                GameController.ins.currPly.CognitoHazard(false);
                GameController.ins.currPly.StopLook();
                Debug.Log("Turning off cognitoeffect");
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
        shesamaniac = false;

        base.EventFinished();

    }
}
