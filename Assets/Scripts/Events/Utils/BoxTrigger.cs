using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTrigger : MonoBehaviour
{
    bool Triggered;
    public bool autoFalse = false;

    // Update is called once per frame
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            Triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            Triggered = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            Triggered = true;
    }

    private void LateUpdate()
    {
        if (autoFalse)
            Triggered = false;
    }

    public bool GetState()
    {
        bool value = Triggered;
        
        return value;
        
    }
}
