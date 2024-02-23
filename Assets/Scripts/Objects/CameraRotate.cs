using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public Transform horizontalPivot;
    public float maxPan = 0.0f;
    public float maxTilt = 0.0f;
    public float speed = 2.0f;

    private float panSoFar = 0.0f;
    private bool panBack = false;
    private bool firstRotation = true;

    // Update is called once per frame
    void Update()
    {
        RotateCamera();
    }

    void RotateCamera()
    {
        if (horizontalPivot && maxPan > 0)
        {
            if (!panBack)
            {
                horizontalPivot.Rotate(0, (Time.deltaTime * speed), 0);
                panSoFar += Time.deltaTime * speed;
                if (firstRotation)
                {
                    if (panSoFar >= maxPan)
                    {
                        panSoFar = 0;
                        panBack = true;
                        firstRotation = false;
                    }
                    return;
                }
                else
                {
                    if (panSoFar >= maxPan * 2)
                    {
                        panSoFar = 0;
                        panBack = true;
                    }
                }
            }
            else
            {
                horizontalPivot.Rotate(0, -(Time.deltaTime * speed), 0);
                panSoFar += Time.deltaTime * speed;

                if (panSoFar >= maxPan * 2)
                {
                    panSoFar = 0;
                    panBack = false;
                }
            }
        }
    }
}
