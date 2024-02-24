using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new Keycard", menuName = "Items/Keycard")]
public class Equipable_Key : Equipable_Wear
{
    public int level;

    public override void Use(ref GameItem currItem)
    {
        PlayerControl player = GameController.ins.player.GetComponent<PlayerControl>();

        if (player.equipment[(int)this.part] == null || ItemController.instance.items[player.equipment[(int)this.part].itemFileName].itemName != this.itemName)
            player.ACT_Equip(currItem);
        else
            player.ACT_UnEquip(part);

    }

}
