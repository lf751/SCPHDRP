using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventChance
{
    public int Chance;
    public string EventHandler;
    public Transform pos;
}


public class EventHandler : MonoBehaviour
{
    public EventChance[] EventList;


    public string UniqueHandler;
    public bool Spawned = false;
    public string SpecialName;
    public Transform UniquePos;
    public int EventChosen = -1;
    worldPos roomPos;
    GameObject Event;
    bool IsActive, hasUnique;

    public int EventSet()
    {
        EventChosen = -1;
        for (int i=0; i < EventList.Length; i++)
        {
            if (EventList[i].Chance > Random.Range(0,100))
            {
                EventChosen = i;
                break;
            }
        }
        return (EventChosen);
    }

    public void ForceEvent(int forcedevent)
    {
        EventChosen = forcedevent;
    }

    public void EventSpecial()
    {
        EventChosen = -2;
    }

    public string GetEventName()
    {
        return (SpecialName);
    }

    public void EventLoad(int x, int y, bool isDone)
    {
        
        if (Spawned == false)
        {
            if (EventChosen == -2)
                InternalLoad(UniqueHandler, x, y, UniquePos);

            if (EventChosen >= 0)
                InternalLoad(EventList[EventChosen].EventHandler, x, y, EventList[EventChosen].pos);

            if (isDone)
                Event.GetComponent<Event_Parent>().EventFinished();


            Spawned = true;
        }
    }

    void InternalLoad(string scp_event, int x, int y, Transform pos)
    {
        Event = Resources.Load<GameObject>(string.Concat("Events/", scp_event));

        Debug.Log("Evento " + scp_event + " cargado en " + x + " " + y);
        
        if (pos == null)
        {
            Event = Instantiate(Event, this.transform.position, this.transform.rotation, GameController.instance.eventParent.transform);

        }
        else
            Event = Instantiate(Event, pos.position, pos.rotation, GameController.instance.eventParent.transform);

        Event.GetComponent<Event_Parent>().x = x;
        Event.GetComponent<Event_Parent>().y = y;
        Event.GetComponent<Event_Parent>().EventLoad();
    }

    public void EventStart()
    {
        if (Event)
        {
            Debug.Log("Evento iniciado " + Event.name);
            Event.GetComponent<Event_Parent>().EventStart();
        }
    }

    public void EventUnLoad()
    {
        if (Event)
        {
            Debug.Log("Eliminando evento " + Event.name);
            Event.GetComponent<Event_Parent>().EventUnLoad();
            Destroy(Event);
        }
        Event = null;
        Spawned = false;
        

    }




    /*void EventDone(string scp_event, int x, int y, Transform pos)
    {
        GameObject res_event = Resources.Load<GameObject>(string.Concat("Events/", scp_event));
        if (pos == null)
        {
            res_event = Instantiate(res_event, this.transform.position, this.transform.rotation, GameController.instance.eventParent.transform);
        }
        else
            res_event = Instantiate(res_event, pos.position, pos.rotation, GameController.instance.eventParent.transform);
        res_event.GetComponent<Event_Parent>().x = x;
        res_event.GetComponent<Event_Parent>().y = y;
        res_event.GetComponent<Event_Parent>().EventFinished();
    }
    */
}
