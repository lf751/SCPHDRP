using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Radio", menuName = "Items/Radio")]
public class Equipable_Radio : Equipable_Elec
{
    public override void Use(ref gameItem currItem)
    {
        this.part = bodyPart.Hand;
        base.Use(ref currItem);
    }

    public override void OnEquip(ref gameItem currItem)
    {
        base.OnEquip(ref currItem);
        SCP_UI.instance.radio.StartRadio();
    }

    public override void OnDequip(ref gameItem currItem)
    {
        base.OnDequip(ref currItem);
        SCP_UI.instance.radio.StopRadio();
    }


}
