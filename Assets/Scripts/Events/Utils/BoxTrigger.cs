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
        {
            //Debug.Log("Player exited", this.gameObject);
            Triggered = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Triggered = true;
            //Debug.Log("Player entered trigger", this.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
            Triggered = true;
    }

    private void FixedUpdate()
    {
        if (autoFalse)
        {
            Triggered = false; 
            //Debug.Log("Auto Updating boxtrigger", this.gameObject);
        }
    }

    public bool GetState()
    {
        return Triggered;
        
    }
}
