using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Persistent : MonoBehaviour
{
    protected int id;
    protected bool State;
    public bool ignoreSave;

    public virtual void Start()
    {
        if (!ignoreSave)
        {
            id = GameController.instance.GetObjectID();
            transform.parent = GameController.instance.persParent.transform;
            resetState();
        }
    }

    public virtual void resetState()
    {
        Debug.Log("Resetting state of " + this.gameObject.name);
        int newState = GameController.instance.GetObjectState(id);
        if (newState != -1)
        {
            State = newState == 1;
        }
        else
        {
            GameController.instance.SetObjectState(State, id);
        }
    }
}
