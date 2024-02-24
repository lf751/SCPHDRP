using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Prop : Object_Interact
{
    Rigidbody body;
    bool isHolding, stopHold;
    // Start is called before the first frame update
    private void Update()
    {
        if (isHolding == false && stopHold == false)
        {
            StopHold();
            stopHold = true;
        }
    }

    private void LateUpdate()
    {
        isHolding = false;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void Hold()
    {
        isHolding = true;
        stopHold = false;
        transform.position = GameController.ins.currPly.handPos.transform.position + (GameController.ins.currPly.handPos.transform.forward * 0.6f);
        body.isKinematic = true;
        body.useGravity = false;

    }

    void OnCollisionEnter(Collision collision)
    {
        StopHold();
        stopHold = true;
        // Debug-draw all contact points and normals
        /*foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
        }

        // Play a sound if the colliding objects had a big impact.
        if (collision.relativeVelocity.magnitude > 2)
            audioSource.Play();*/
    }

    void StopHold()
    {
        body.isKinematic = false;
        body.useGravity = true;
    }
}
