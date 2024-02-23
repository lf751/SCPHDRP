using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CinemaSwitch : MonoBehaviour
{
    int Current;
    public Volume [] presets;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (SCPInput.instance.playerInput.Gameplay.Radio1.triggered)
        {
            presets[Current].weight = 0;
            presets[0].weight = 1;
            Current = 0;
        }
        if (SCPInput.instance.playerInput.Gameplay.Radio2.triggered)
        {
            presets[Current].weight = 0;
            presets[1].weight = 1;
            Current = 1;
        }
        if (SCPInput.instance.playerInput.Gameplay.Radio3.triggered)
        {
            presets[Current].weight = 0;
            presets[2].weight = 1;
            Current = 2;
        }
        if (SCPInput.instance.playerInput.Gameplay.Radio4.triggered)
        {
            presets[Current].weight = 0;
        }
    }
}
