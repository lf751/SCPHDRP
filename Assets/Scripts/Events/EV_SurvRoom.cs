using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_SurvRoom : Event_Parent
{
    public ScreenRenderer blockscreen, blockscreen2;
    public Object_LeverV lever;
    public BoxTrigger endtrigger;
    public Transform spawn1;
    public Transform reach049;
    public AudioClip dialog1, dialog2, eventMusic;
    public float framerate = 15;
    public AudioSource dialogGenerator;
    int status = 0;
    float Timer = 10;
    public float timeForReach, timeForSecondDialog, timeForEnd;
    
    // Start is called before the first frame update
    // Update is called once per frame
    void Update()
    {
        if (isStarted)
        {
            EventUpdate();
        }
    }

    public override void EventUpdate()
    {
        if (state < 3 && ((NPC_049)GameController.ins.npcController.mainList[(int)npc.scp049]).seePlayer)
        {
            Debug.Log("I saw u");
            GameController.ins.npcController.mainList[(int)npc.scp049].StopEvent();
            EventFinished();
        }
        /*else
            Debug.Log("I dont found you");*/

        if (status == 0 && lever.On == false)
        {
            GameController.ins.globalBools[0] = true;
            status = 1;
            Timer = 5;
            blockscreen.animate = false;
            blockscreen.SetFrame(0, 0);
            blockscreen2.animate = false;
            blockscreen2.SetFrame(0, 0);
            GameController.ins.canSave = false;
            GameController.ins.npcController.mainList[(int)npc.scp049].Event_Spawn(true, spawn1.position);
        }

        if (status != 0)
            Timer -= Time.deltaTime;

        if (Timer <= 0)
        {
            switch (status)
            {
                case 1:
                    {
                        ((NPC_049)GameController.ins.npcController.mainList[(int)npc.scp049]).evWalkTo(reach049.transform.position);
                        status = 2;

                        Timer = 0.1f;
                        
                        break;
                    }
                case 2:
                    {
                        GameController.ins.ChangeMusic(eventMusic);
                        Timer = timeForReach;
                        status = 3;
                        break;
                    }
                case 3:
                    {
                        ((NPC_049)GameController.ins.npcController.mainList[(int)npc.scp049]).evChangeState(5);
                        SubtitleEngine.instance.playVoice(dialog1.name, true);
                        dialogGenerator.clip = dialog1;
                        dialogGenerator.Play();
                        Timer = timeForSecondDialog;
                        status = 4;
                        break;
                    }
                case 4:
                    {
                        ((NPC_049)GameController.ins.npcController.mainList[(int)npc.scp049]).evChangeState(0);
                        dialogGenerator.clip = dialog2;
                        SubtitleEngine.instance.playVoice(dialog2.name, true);
                        dialogGenerator.Play();
                        Timer = timeForEnd;
                        status = 5;
                        break;
                    }
                case 5:
                    {
                        status = 6;
                        GameController.ins.npcController.mainList[(int)npc.scp049].StopEvent();
                        EventFinished();
                        break;
                    }



            }

        }

        /*if (Time.frameCount % framerate == 0)
        {
            
        }*/

        if (endtrigger.GetState() && status != 0)
        {
            GameController.ins.npcController.mainList[(int)npc.scp049].StopEvent();
            EventFinished();
        }

    }

    public override void EventFinished()
    {
        base.EventFinished();
        GameController.ins.DefMusic();
        GameController.ins.canSave = true;
        isStarted = false;
    }
}
