﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Document", menuName = "Items/Document")]
public class Document_Equipable : Equipable_Wear
{
    public string filename;

    public override void Use(ref GameItem currItem)
    {
        this.part = bodyPart.Hand;
        base.Use(ref currItem);
    }


    public override void OnEquip(ref GameItem currItem)
    {
        this.Overlay = Resources.Load<Sprite>(string.Concat("Items/Docs/", filename));

        /*if (this.itemName == "doc372")
            SCP_UI.instance.bottomScrible.text = GameController.instance.globalStrings[0];*/
    }

    public override void OnDequip(ref GameItem currItem)
    {
        Resources.UnloadAsset(this.Overlay);
    }

}
