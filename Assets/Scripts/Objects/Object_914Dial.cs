using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_914Dial : Object_Interact
{
    public int Option = 0;
    float changing = 0;
    bool start, change; 
    // Start is called before the first frame update
    void Update()
    {
    }

    // Update is called once per frame
    public override void Hold()
    {
        if(SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().x != 0)
        {
            changing -= Time.deltaTime;
            if (changing <= 0)
            {
                change = true;
                start = false;
            }

            if (change == true)
            {

                if (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().x > 0)
                {
                    Option = Mathf.Clamp(Option + 1, 0, 5);
                }
                if (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().x < 0)
                {
                    Option = Mathf.Clamp(Option - 1, 0, 5);
                }
                change = false;
            }
        }

        if (start == false)
        {
            changing = 0.3f;
            start = true;
        }


    }
}
