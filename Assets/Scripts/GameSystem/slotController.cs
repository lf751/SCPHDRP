using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class slotController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    Vector3 orpos;
    bool dragging;
    public int id;
    ItemController cont;

    private void Start()
    {
        orpos = transform.position;
        cont = ItemController.instance;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (id != -1)
        {
            transform.position = SCPInput.instance.playerInput.UI.Pointer.ReadValue<Vector2>();
            dragging = true;
            cont.currdrag = id;
            GetComponent<Image>().raycastTarget = false;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = orpos;
        GetComponent<Image>().raycastTarget = true;
        dragging = false;

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
                slotMove();
                cont.UpdateInv();
                return;
            }
            if (cont.currentItem[cont.currhover] != null && cont.currentEquip[cont.currhover] != true && cont.currentEquip[id] != true)
            {
                if (cont.items[cont.currentItem[cont.currhover].itemFileName].Mix(ref cont.currentItem[cont.currhover], ref cont.currentItem[id]))
                {
                    cont.currentItem[id] = null;
                }
            }
        }

        cont.UpdateInv();
    }

    public void UnEquip(bool dontToggle = false)
    {
        GameController.instance.player.GetComponent<Player_Control>().DropItem(cont.currentItem[id]);
        SCP_UI.instance.ItemSFX(cont.items[cont.currentItem[id].itemFileName].SFX);
        cont.currentItem[id] = null;
        if(!dontToggle)
            SCP_UI.instance.ToggleInventory();
    }



    public void updateInfo()
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
            if (cont.items[cont.currentItem[id].itemFileName] is Item_Clipboard clipboard)
            {
                if (cont.currentItem[id].valInt != -1)
                {
                    if (cont.IsEmpty(cont.currentItem[id].valInt))
                    {
                        Item_Clipboard clippy = clipboard;
                        currIcon = clippy.nodoc;
                    }
                }
                else
                {
                    Item_Clipboard clippy = clipboard;
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
        if (cont.currentItem[id]!=null && !dragging)
        {
            Item currItem = cont.items[cont.currentItem[id].itemFileName];
            int cacheinv = cont.currInv;
            SCP_UI.instance.ItemSFX(currItem.SFX);
            bool dontclose = currItem.keepInv;
            currItem.Use(ref cont.currentItem[cont.currhover]);
            if (cont.currInv == cacheinv && currItem.deleteUse == true)
            {
                if (currItem.isUnique)
                    SubtitleEngine.instance.playFormatted("playStrings", "play_used_uni", "itemStrings", currItem.getName());
                else if (currItem.isFem)
                    SubtitleEngine.instance.playFormatted("playStrings", "play_used_fem", "itemStrings", currItem.getName());
                else
                    SubtitleEngine.instance.playFormatted("playStrings", "play_used_male", "itemStrings", currItem.getName());

                cont.currentItem[id] = null;
            }
            if (!dontclose&&!dontToggle)
                SCP_UI.instance.ToggleInventory();
        }
        updateInfo();
    }

    public void Hover()
    {
        cont.currhover = id;
    }
    public void slotMove()
    {
        cont.currentItem[cont.currhover] = cont.currentItem[id];
        cont.currentItem[id] = null;
    }
}
