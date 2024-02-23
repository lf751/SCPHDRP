using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box106 : MonoBehaviour
{
    public float speedUpDown = 1, maxMoveSpeed;
    public float distanceUpDown = 1, distanceLeftRight, distanceForwardBackwards, rotaLimiter;

    public bool isFloating=true;

    public Transform notFloatingPos;
    Vector3 startPosition, target;
    Quaternion startRotation, rotaTarget;
    float random1, random2;
    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        target = startPosition;
        rotaTarget = transform.rotation;
        random1 = Random.Range(-1.5f, 1.5f);
        random2 = Random.Range(-1.5f, 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        float x, y, z;
        x = (Mathf.Sin(speedUpDown * (Time.time * random1)) * distanceLeftRight);
        y = (Mathf.Sin(speedUpDown * Time.time) * distanceUpDown);
        z = ((Mathf.Sin(speedUpDown * (Time.time * random2))) * distanceForwardBackwards);

        if (isFloating)
        {
            target = new Vector3(startPosition.x + x, startPosition.y + y, startPosition.z + z);
            rotaTarget = Quaternion.Euler(startRotation.eulerAngles.x + (x * rotaLimiter), startRotation.eulerAngles.y + (y * rotaLimiter), startRotation.eulerAngles.z + (z * rotaLimiter));
        }
        else
        {
            target = notFloatingPos.position;
            rotaTarget = startRotation;
        }
        
        transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, target, maxMoveSpeed * Time.deltaTime), Quaternion.RotateTowards(transform.rotation, rotaTarget, maxMoveSpeed * Time.deltaTime));
    }
}
