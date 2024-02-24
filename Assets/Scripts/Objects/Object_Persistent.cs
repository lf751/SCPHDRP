using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Persistent : MonoBehaviour
{
    protected int id;
    protected int State;
    public bool ignoreSave;

    public virtual void Start()
    {
        if (!ignoreSave)
        {
            id = GameController.ins.GetObjectID();
            transform.parent = GameController.ins.persParent.transform;
            ResetState();
        }
    }

    public virtual void ResetState()
    {
        //Debug.Log("Resetting state of " + this.gameObject.name);
        int newState = GameController.ins.GetObjectState(id);
        if (newState != -1)
        {
            State = newState;
        }
        else
        {
            GameController.ins.SetObjectState(State, id);
        }
    }
}
