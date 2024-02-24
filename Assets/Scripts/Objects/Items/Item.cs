using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Item : ScriptableObject
{
    public Vector3 colCenter = new Vector3(-0.04888153f, -0.03777958f, -0.05219725f);
    public Vector3 colSize = new Vector3(0.250877f, 0.1208749f, 0.4735436f);
    public float mass = 1;
    public string itemName;
    public Sprite icon;
    public bool deleteUse, hasEffect, isFem = false, isUnique = false, keepInv = false;
    public List<EfectTable> Effects;
    public GameObject ItemModel;
    public float valueFloat;
    public int valueInt;
    public int SFX;
    public float useTime = 0f;
    public virtual void Use(ref GameItem currItem)
    {

    }

    public virtual bool Mix(ref GameItem currItem, ref GameItem toMix)
    {
        return false;
    }

    public virtual string getName()
    {
        return itemName;
    }


}
