using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Screen : Object_Captive
{
    // Start is called before the first frame update
    public Sprite screen;

    // Update is called once per frame
    public override void StartCapture()
    {
        SCP_UI.instance.ScreenText.sprite = screen;
        SCP_UI.instance.ToggleScreen();
    }

    public override void EndCaptive()
    {
        SCP_UI.instance.ToggleScreen();
    }

}
