using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Hand", menuName = "Items/Hand")]
public class Equipable_Hand : Equipable_Wear
{
    public int handID;

    public override void Use(ref GameItem currItem)
    {
        PlayerControl player = GameController.ins.player.GetComponent<PlayerControl>();

        if (player.equipment[(int)this.part] == null || ItemController.instance.items[player.equipment[(int)this.part].itemFileName].itemName != this.itemName)
            player.ACT_Equip(currItem);
        else
            player.ACT_UnEquip(part);

    }

}
