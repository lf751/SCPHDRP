using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PD_NextSection : MonoBehaviour
{
    public BoxTrigger trigger;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (trigger.GetState())
        {
            PD_Teleports.instance.Teleport();
        }
    }
}
