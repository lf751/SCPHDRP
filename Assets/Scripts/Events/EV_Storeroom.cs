using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Storeroom : Event_Parent
{
    // Start is called before the first frame update
    public AudioClip Voice1, Voice2, Horror, LightsOut, cronch, vent;
    public EV_Puppet_Controller guard, sci;
    public Transform[] Path1;
    public Transform Spawn173, lookat;
    public float time1, time2, time3, time4, time5;
    float timer = 0;
    bool setup;
    int status=0;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isStarted)
            EventUpdate();
    }

    public override void EventUpdate()
    {
        base.EventUpdate();

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            switch(status)
            {
                case 0:
                    {
                        sci.SetPath(Path1);
                        guard.PlaySound(Voice1,true);
                        guard.SetRota(sci.gameObject.transform);
                        timer = time1;
                        status = 1;
                        break;
                    }
                case 1:
                    {
                        GameController.instance.GlobalSFX.PlayOneShot(vent);
                        sci.PlaySound(Voice2,true);
                        guard.SetRota(lookat);
                        sci.SetRota(lookat);
                        timer = time2;
                        status = 2;
                        break;
                    }
                case 2:
                    {
                        GameController.instance.player.GetComponent<Player_Control>().FakeBlink(time3 + time4 + time5);
                        timer = time3;
                        status = 3;
                        break;
                    }
                case 3:
                    {
                        sci.PlaySFX(cronch);
                        sci.AnimTrigger(-1, true);
                        timer = time4;
                        status = 4;
                        break;
                    }
                case 4:
                    {
                        guard.PlaySFX(cronch);
                        guard.AnimTrigger(-2, true);
                        GameController.instance.npcController.mainList[(int)npc.scp173].Event_Spawn(true, Spawn173.transform.position);
                        GameController.instance.npcController.mainList[(int)npc.scp173].transform.rotation = Spawn173.transform.rotation;
                        timer = time5;
                        status = 5;
                        break;
                    }
                case 5:
                    {
                        EventFinished();
                        break;
                    }
            }
        }

    }


    public override void EventFinished()
    {
        base.EventFinished();
        guard.AnimTrigger(-2, true);
        sci.AnimTrigger(-1, true);
        isStarted = false;
    }
}
