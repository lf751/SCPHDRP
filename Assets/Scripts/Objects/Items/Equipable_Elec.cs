using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipable_Elec : Equipable_Wear
{
    public bool SpendBattery = true;
    public bool ReceiveBattery = true;
    public float SpendFactor = 0.5f;
    public override void Use(ref GameItem currItem)
    {
        PlayerControl player = GameController.ins.player.GetComponent<PlayerControl>();
        if (player.equipment[(int)this.part] == null || ItemController.instance.items[player.equipment[(int)this.part].itemFileName].itemName != this.itemName)
            player.ACT_Equip(currItem);
        else
            player.ACT_UnEquip(part);
    }

    public override bool Mix(ref GameItem currItem, ref GameItem toMix)
    {
        if (!ReceiveBattery)
            return (false);
        if (ItemController.instance.items[toMix.itemFileName].itemName.Equals("bat_nor"))
        {
            currItem.valFloat = 100;
            return (true);
        }
        else
            return (false);
    }
}
