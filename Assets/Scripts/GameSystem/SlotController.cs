using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    Vector3 orpos;
    bool dragging;
    public int id;
    ItemController cont;

    private void Awake()
    {
        //Debug.Log("slot " + id + " postion: " + transform.position);
        orpos = transform.position;
        cont = ItemController.instance;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging)
            orpos = transform.position;
        if (id != -1)
        {
            transform.position = eventData.position;
            dragging = true;
            cont.currdrag = id;
            GetComponent<Image>().raycastTarget = false;
            // Debug.Log("Draggin " + id);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = orpos;
        GetComponent<Image>().raycastTarget = true;
        dragging = false;
        //Debug.Log("Stop draggin " + id);

        bool foundItemFromInv = false;
        PlayerControl currPly = GameController.ins.currPly;
        if (cont.items[cont.currentItem[id].itemFileName] is Item_Clipboard)
        {
            GameItem itm = ItemController.instance.currentItem[id];
            if (itm.valInt == currPly.handInv)
                foundItemFromInv = true;
            if (itm.valInt == currPly.headInv)
                foundItemFromInv = true;
            if (itm.valInt == currPly.anyInv)
                foundItemFromInv = true;
            if (itm.valInt == currPly.bodyInv)
                foundItemFromInv = true;
        }

        if (cont.currhover != -1)
        {
            if (cont.items[cont.currentItem[cont.currhover].itemFileName] is Item_Clipboard)
            {
                GameItem itm = ItemController.instance.currentItem[cont.currhover];
                if (itm.valInt == currPly.handInv)
                    foundItemFromInv = true;
                if (itm.valInt == currPly.headInv)
                    foundItemFromInv = true;
                if (itm.valInt == currPly.anyInv)
                    foundItemFromInv = true;
                if (itm.valInt == currPly.bodyInv)
                    foundItemFromInv = true;
            }
        }

        if (!foundItemFromInv)
        {
            if (cont.currhover == -1)
            {
                if (cont.currentEquip[id] == false && cont.currentItem[id] != null)
                {
                    UnEquip();
                }
            }
            else
            {
                if (cont.currhover != -1 && cont.currentItem[cont.currhover] == null && cont.currentEquip[cont.currhover] != true && cont.currentEquip[id] != true)
                {
                    SlotMove();
                    cont.UpdateInv();
                    return;
                }
                if (cont.currentItem[cont.currhover] != null && cont.currentEquip[cont.currhover] != true && cont.currentEquip[id] != true)
                {
                    if (cont.items[cont.currentItem[cont.currhover].itemFileName].Mix(ref cont.currentItem[cont.currhover], ref cont.currentItem[id]))
                    {
                        cont.currentItem[id] = null;
                    }
                    cont.StopItemUse();
                }
            }
        }

        cont.UpdateInv();
    }

    public void UnEquip(bool dontToggle = false)
    {
        cont.StopItemUse();
        GameController.ins.player.GetComponent<PlayerControl>().DropItem(cont.currentItem[id]);
        SCP_UI.instance.ItemSFX(cont.items[cont.currentItem[id].itemFileName].SFX);
        cont.currentItem[id] = null;
        if (!dontToggle)
            SCP_UI.instance.ToggleInventory();
    }



    public void UpdateInfo()
    {
        Text displayText = transform.Find("Text").GetComponent<Text>();
        Image displayImage = transform.Find("Image").GetComponent<Image>();
        Sprite currIcon;

        if (cont.currentItem[id] != null)
        {
            currIcon = cont.items[cont.currentItem[id].itemFileName].icon;
            if (!cont.currentEquip[id])
                displayText.text = Localization.GetString("itemStrings", cont.items[cont.currentItem[id].itemFileName].getName());
            else
                displayText.text = string.Format(Localization.GetString("playStrings", "play_equiped"), Localization.GetString("itemStrings", cont.items[cont.currentItem[id].itemFileName].getName()));
            if (cont.items[cont.currentItem[id].itemFileName] is Item_Clipboard)
            {
                if (cont.currentItem[id].valInt != -1)
                {
                    if (cont.IsEmpty(cont.currentItem[id].valInt))
                    {
                        Item_Clipboard clippy = (Item_Clipboard)cont.items[cont.currentItem[id].itemFileName];
                        currIcon = clippy.nodoc;
                    }
                }
                else
                {
                    Item_Clipboard clippy = (Item_Clipboard)cont.items[cont.currentItem[id].itemFileName];
                    currIcon = clippy.nodoc;
                }
            }



            displayImage.sprite = currIcon;
            displayImage.color = Color.white;
        }
        else
        {
            displayText.text = "";
            displayImage.sprite = null;
            displayImage.color = Color.black;
        }
    }


    public void Use(bool dontToggle = false)
    {
        if (cont.currentItem[id] != null && !dragging)
        {
            cont.Use(id, cont.currInv, false, dontToggle);
        }
        UpdateInfo();
    }

    public void Hover()
    {
        cont.currhover = id;
    }
    public void SlotMove()
    {
        cont.StopItemUse();
        cont.currentItem[cont.currhover] = cont.currentItem[id];
        cont.currentItem[id] = null;
    }
}
