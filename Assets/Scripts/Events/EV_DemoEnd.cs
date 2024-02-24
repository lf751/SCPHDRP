using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EV_DemoEnd : Event_Parent
{
    public AudioClip DemoEnd;
    public float TimerEnd;
    float Timer;
    public BoxTrigger trigger;
    public bool Started = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

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
        base.EventUpdate();

        if (Started)
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                
                Started = false;
                SceneManager.LoadScene(5);
            }
        }

        if (GameController.ins.globalBools[2])
        {
            if (trigger.GetState() && !Started)
            {
                Started = true;
                Timer = TimerEnd;
                GameController.ins.GlobalSFX.PlayOneShot(DemoEnd);
                LoadingSystem.instance.FadeOut(1, new Vector3Int(0, 0, 0));
                GameController.ins.globalBools[2] = false;
                GameController.ins.Action_QuickSave();

            }
        }
        else if (!Started)
            EventFinished();

    }

    public override void EventFinished()
    {
        base.EventFinished();
        isStarted = false;
    }
}
