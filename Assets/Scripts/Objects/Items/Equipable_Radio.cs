using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Radio", menuName = "Items/Radio")]
public class Equipable_Radio : Equipable_Elec
{
    public override void Use(ref GameItem currItem)
    {
        this.part = bodyPart.Hand;
        base.Use(ref currItem);
    }

    public override void OnEquip(ref GameItem currItem)
    {
        base.OnEquip(ref currItem);
        SCP_UI.instance.radio.StartRadio();
    }

    public override void OnDequip(ref GameItem currItem)
    {
        base.OnDequip(ref currItem);
        SCP_UI.instance.radio.StopRadio();
    }


}
