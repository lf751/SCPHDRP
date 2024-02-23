using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceActivate : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<Distance_Object>().Spawn();
    }

    private void OnTriggerExit(Collider other)
    {
        other.GetComponent<Distance_Object>().UnSpawn();
    }


}
