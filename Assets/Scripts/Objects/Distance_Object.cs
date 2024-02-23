using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distance_Object : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Contains;

    public void Spawn()
    {
        Contains.SetActive(true);
    }

    public void UnSpawn()
    {
        Contains.SetActive(false);
    }

}
