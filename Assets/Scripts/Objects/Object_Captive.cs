using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Captive : Object_Interact
{
    public override void Pressed()
    {
        base.Pressed();
        GameController.ins.currPly.CaptureObject(this);
        StartCapture();
    }

    public virtual void StartCapture()
    {

    }

    public virtual void EndCaptive()
    {

    }
}
