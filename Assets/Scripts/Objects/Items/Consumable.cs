using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu (fileName = "new Consumable", menuName = "Items/Consumable")]
public class Consumable : Item
{
    public override void Use(ref GameItem currItem)
    {
        GameController.ins.currPly.SetEffect(this);
    }

}
