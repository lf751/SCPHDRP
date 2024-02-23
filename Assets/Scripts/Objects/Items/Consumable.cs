using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu (fileName = "new Consumable", menuName = "Items/Consumable")]
public class Consumable : Item
{
    public override void Use(ref gameItem currItem)
    {
        GameController.instance.playercache.SetEffect(this);
    }

}
