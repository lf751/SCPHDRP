using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_light_testroom : Event_Parent
{
    public GameObject Anchor1, Anchor2, SCP, Screen;
    public float Timer;
    bool StopTimer=false, ActiveTimer=false;
    public AudioClip [] SFX;

    // Update is called once per frame

    private void LateUpdate()
    {
        if (isStarted == true)
            EventUpdate();
    }

    public override void EventUpdate()
    {   if (ActiveTimer)
            Timer -= Time.deltaTime;
        if (Timer <= 0.0f && StopTimer == false)
        {
            if (GameController.ins.player.GetComponent<PlayerControl>().IsBlinking())
            {
                GameController.ins.player.GetComponent<PlayerControl>().FakeBlink(0.3f);
                GameController.ins.npcController.mainList[(int)npc.scp173].Event_Spawn(true, Anchor2.transform.position);
                StopTimer = true;
                Screen.SetActive(false);
                GameController.ins.PlayHorror(SFX[0],Anchor2.transform, npc.none);
                GameController.ins.PlayHorror(SFX[1],Anchor2.transform, npc.none);
                SCP_UI.instance.ShowTutorial("tutoinv2");
                EventFinished();
            }
        }
    }


    public override void EventStart()
    {
        base.EventStart();
        GameController.ins.npcController.mainList[(int)npc.scp173].Event_Spawn(true, Anchor1.transform.position);
    }

    public override void EventFinished()
    {
        base.EventFinished();
        Screen.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            ActiveTimer = true;
    }
}
