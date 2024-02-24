using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Elevator : Object_Persistent
{
    public bool isDisabled = false;
    public Transform Floor1, Floor2;
    public Object_Button_Trigger Out1, Out2, Switch1, Switch2;
    public Object_Door Door1, Door2;
    public GameObject Door1Objects, Door2Objects;
    public bool FloorUp;
    public float MovingTime;
    bool Ignoreinputs = false;
    bool insideElev;
    bool calledFromUp;
    public AudioClip elev, ding;
    bool soundPlayed = true;

    float Timer;
    public override void Start()
    {
        State = FloorUp ? 1 : 0;
        base.Start();

    }

    public override void ResetState()
    {
        base.ResetState();
        FloorUp = State == 1 ? true : false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDisabled)
        {
            if (Out1.GetComponent<Object_Button_Trigger>().activated == true && !Ignoreinputs && Timer <= 0)
            {
                if (!FloorUp)
                {
                    CloseDoors();
                    Timer = MovingTime;
                    insideElev = false;
                    Ignoreinputs = true;
                    soundPlayed = false;
                }
                else
                    Door1.DoorSwitch();
            }

            if (Out2.GetComponent<Object_Button_Trigger>().activated == true && !Ignoreinputs && Timer <= 0)
            {
                if (FloorUp)
                {
                    CloseDoors();
                    Timer = MovingTime;
                    insideElev = false;
                    Ignoreinputs = true;
                    soundPlayed = false;
                }
                else
                    Door2.DoorSwitch();
            }

            if (((Switch1.GetComponent<Object_Button_Trigger>().activated == true && Door1.IsOpen) || (Switch2.GetComponent<Object_Button_Trigger>().activated == true && Door2.IsOpen)) && !Ignoreinputs && Timer <= 0)
            {
                //SwitchFloor(Floor1.transform, Floor2.transform);
                CloseDoors();
                Door1.isDisabled = true;
                Door2.isDisabled = true;
                Timer = MovingTime;
                insideElev = true;
                Ignoreinputs = true;
                soundPlayed = false;
            }
        }


        Timer -= Time.deltaTime;

        if (Timer <= (MovingTime - 2) && !soundPlayed)
        {
            GameController.ins.GlobalSFX.PlayOneShot(elev);
            soundPlayed = true;
        }

        if (Timer <= 3 && Ignoreinputs)
        {
            Door1.isDisabled = false;
            Door2.isDisabled = false;

            FloorUp = !FloorUp;
            State = FloorUp ? 1 : 0;
            GameController.ins.SetObjectState(State, id);

            if (insideElev)
            {
                if (FloorUp)
                {
                    Door1Objects.SetActive(true);
                    SwitchFloor(Floor2, Floor1);
                }
                else
                {
                    Door2Objects.SetActive(true);
                    SwitchFloor(Floor1, Floor2);
                }
            }

            if (FloorUp)
            {
                Door1.DoorSwitch();
                GameController.ins.holdRoom = false;
            }
            else
            {
                Door2.DoorSwitch();
                GameController.ins.holdRoom = true;
            }

            GameController.ins.GlobalSFX.PlayOneShot(ding);
            Ignoreinputs = false;
        }
    }

    void SwitchFloor(Transform start, Transform end)
    {
        //yield return null;
        GameObject objPlayer = GameController.ins.player;
        objPlayer.GetComponent<PlayerControl>().playerWarp((end.transform.position + ((end.transform.rotation * Quaternion.Inverse(start.transform.rotation)) * (objPlayer.transform.position - start.position))), end.transform.eulerAngles.y - start.transform.eulerAngles.y);

        //Debug.Log("Diferencia de Rotacion: " + (end.transform.eulerAngles.y - start.transform.eulerAngles.y));

    }

    public void OpenDoors()
    {
        if (!Door1.switchOpen)
            Door1.DoorSwitch();
        if (!Door2.switchOpen)
            Door2.DoorSwitch();
    }

    void CloseDoors()
    {
        if (Door1.switchOpen)
            Door1.DoorSwitch();
        if (Door2.switchOpen)
            Door2.DoorSwitch();
    }
}
