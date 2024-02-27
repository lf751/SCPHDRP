using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPC_Data
{
    public SeriVector Pos, Target, Rotation;
    public npctype type;
    public npcstate state= npcstate.alive;
    public bool isActive=false;
    public int[] npcvalue = new int[5];
}

public class Map_NPC : MonoBehaviour
{
    [HideInInspector]
    public NPC_Data data;

    public string charName;
    // Start is called before the first frame update
    public virtual void createData()
    {
        data.Pos = new SeriVector(transform.position.x, transform.position.y, transform.position.z);
    }

    public virtual void setData(NPC_Data state)
    {
        data = state;
        transform.position = state.Pos.toVector3();
        transform.rotation = Quaternion.Euler(state.Rotation.toVector3());
    }

    public virtual NPC_Data getData()
    {
        data.Pos = new SeriVector(transform.position.x, transform.position.y, transform.position.z);
        data.Rotation = SeriVector.fromVector3(transform.rotation.eulerAngles);
        return (data);
    }

    public virtual void NpcDisable()
    {
        data.isActive = false;
    }

    public virtual void NpcEnable()
    {
        data.isActive = true;
    }
}
