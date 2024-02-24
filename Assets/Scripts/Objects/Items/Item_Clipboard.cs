using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new Clipboard", menuName = "Items/Clipboard")]
public class Item_Clipboard : Item
{
    public Sprite doc, nodoc;
    // Start is called before the first frame update
    public override void Use(ref GameItem currItem)
    {
        //Debug.Log("Current inv = " + currItem.valInt);
        if (currItem.valInt == -1)
        {
            currItem.valInt = ItemController.instance.invs.Count;
            ItemController.instance.NewInv();
        }
        ItemController.instance.ChangeInv(currItem.valInt);
    }
    public override bool Mix(ref GameItem currItem, ref GameItem toMix)
    {
        Item itDef = ItemController.instance.items[toMix.itemFileName];
        if (itDef is Document_Equipable || itDef is Equipable_Key || itDef is Equipable_DocumentLoad)
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
