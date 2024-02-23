using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Roam_NPC : Map_NPC
{
    public bool isEvent;

    public virtual void Spawn(bool beActive, Vector3 warppoint)
    {
    }

    public virtual void Event_Spawn(bool instant, Vector3 warppoint)
    {
    }

    public virtual void StopEvent()
    {
    }

    public virtual void SetAgroLevel(int level)
    {
        data.npcvalue[0] = level;
    }

    public virtual void UnSpawn()
    {
    }
}
