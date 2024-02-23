using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_Recursive : Event_Parent
{
    public Transform rightBase, leftBase;
    public BoxTrigger rightTrigger, leftTrigger;
    Object_Door doorRight1, doorRight2, doorRight3, doorLeft1, doorLeft2, doorLeft3;

    public override void EventLoad()
    {
        base.EventLoad();
        doorRight1 = GameController.instance.getCutsceneObject(x, y, 0).GetComponent<Object_Door>();
        doorRight2 = GameController.instance.getCutsceneObject(x, y, 1).GetComponent<Object_Door>();
        doorRight3 = GameController.instance.getCutsceneObject(x, y, 2).GetComponent<Object_Door>();

        doorLeft1 = GameController.instance.getCutsceneObject(x, y, 3).GetComponent<Object_Door>();
        doorLeft2 = GameController.instance.getCutsceneObject(x, y, 4).GetComponent<Object_Door>();
        doorLeft3 = GameController.instance.getCutsceneObject(x, y, 5).GetComponent<Object_Door>();
    }

    private void Update()
    {
        if (isStarted)
            EventUpdate();
    }

    public override void EventUpdate()
    {
        base.EventUpdate();

        if(rightTrigger.GetState())
        {
            //doorLeft3.InstantSet(doorLeft2.switchOpen);
            doorLeft1.InstantSet(doorRight1.switchOpen);
            doorLeft2.InstantSet(true);
            doorRight1.InstantSet(false);
            doorRight2.InstantSet(false);
            Switch(rightBase, leftBase);
        }

        if (leftTrigger.GetState())
        {
            //doorRight3.InstantSet(doorRight1.switchOpen);
            doorRight2.InstantSet(doorLeft2.switchOpen);
            doorRight1.InstantSet(true);
            doorLeft2.InstantSet(false);
            doorLeft1.InstantSet(false);
            Switch(leftBase, rightBase);
        }

    }

    // Start is called before the first frame update
    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/

    void Switch(Transform start, Transform end)
    {
        GameController.instance.playercache.playerWarp((end.transform.position + ((end.transform.rotation * Quaternion.Inverse(start.transform.rotation)) * (GameController.instance.playercache.transform.position - start.position))), end.transform.eulerAngles.y - start.transform.eulerAngles.y);
        Debug.Log("Diferencia de Rotacion: " + (end.transform.eulerAngles.y - start.transform.eulerAngles.y));
    }
}
