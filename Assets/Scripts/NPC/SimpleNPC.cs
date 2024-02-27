using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimpNpcList {bell, teddybear};

public class SimpleNPC : MonoBehaviour
{
    public bool isActive=false;

    // Update is called once per frame
    void Update()
    {
        if (isActive)
            NPCUpdate();
    }

    public virtual void NPCUpdate()
    {
    }

    public virtual void Spawn(Vector3 here, Vector3 rota)
    {
        transform.position = here;
        transform.rotation = Quaternion.Euler(rota);
        isActive = true;
    }

    public virtual void UnSpawn()
    {
        isActive = false;
    }
}
