using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_914Key : Object_Interact
{
    public bool Activated;
    float changing = 0;
    bool start, change;
    // Start is called before the first frame update
    void Update()
    {
        changing -= Time.deltaTime;
        if (changing <= 0)
        {
            change = true;
            start = false;
            Activated = false;
        }
    }

    // Update is called once per frame
    public override void Hold()
    {
        if (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().x != 0)
        {
            if (change == true)
            {
                Activated = true;
                change = false;
            }
        }

        if (start == false)
        {
            changing = 1f;
            start = true;
        }


    }
}
