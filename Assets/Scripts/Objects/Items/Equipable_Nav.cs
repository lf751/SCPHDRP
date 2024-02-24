using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "new SNAV", menuName = "Items/SNAV")]
public class Equipable_Nav : Equipable_Elec
{
    public bool isOnline;
    public bool isRadar=false;

    public override void Use(ref GameItem currItem)
    {
        this.part = bodyPart.Hand;
        base.Use(ref currItem);
    }

    public override void OnEquip(ref GameItem currItem)
    {
        base.OnEquip(ref currItem);
        SCP_UI.instance.SNav.SetActive(true);
    }

    public override void OnDequip(ref GameItem currItem)
    {
        base.OnDequip(ref currItem);
        SCP_UI.instance.SNav.SetActive(false);
    }

}
