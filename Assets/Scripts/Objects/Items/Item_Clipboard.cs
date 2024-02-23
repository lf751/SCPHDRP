using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Clipboard", menuName = "Items/Clipboard")]
public class Item_Clipboard : Item
{
    public Sprite doc, nodoc;
    // Start is called before the first frame update
    public override void Use(ref gameItem currItem)
    {
        //Debug.Log("Current inv = " + currItem.valInt);
        if (currItem.valInt == -1)
        {
            currItem.valInt = ItemController.instance.invs.Count;
            ItemController.instance.NewInv();
        }
        ItemController.instance.ChangeInv(currItem.valInt);
    }
    public override bool Mix(ref gameItem currItem, ref gameItem toMix)
    {
        if (ItemController.instance.items[toMix.itemFileName] is Document_Equipable || ItemController.instance.items[toMix.itemFileName] is Equipable_Key)
        {
            if (currItem.valInt == -1)
            {
                currItem.valInt = ItemController.instance.invs.Count;
                ItemController.instance.NewInv();
            }
            if (ItemController.instance.AddItem(toMix, currItem.valInt)!=-1)
            {
                return true;
            }
            else
                return false;
        }
        else
            return false;
    }



}
