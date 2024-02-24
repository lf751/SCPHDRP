using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distance_Object : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Contains;
    [System.NonSerialized]
    public int idx;
    [System.NonSerialized]
    public bool firstSpawn = true;

    void Awake()
    {
        if (Contains == null)
        {
            return;
        }
        firstSpawn = true;
        Contains.SetActive(false);
        idx = DistanceEngine.instance.AddToEngine(this);
        //Debug.Log("Adding " + gameObject.name + " to group at idx " + idx);
    }

    private void OnDestroy()
    {
        if (Contains == null)
        {
            return;
        }
        //Debug.Log("Removing " + gameObject.name + " to group at idx " + idx);
        DistanceEngine.instance.DeleteFromEngine(this);
    }
}
