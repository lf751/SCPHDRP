using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameItem
{
    public string itemFileName;
    public int valInt = -1;
    public float valFloat = -1;

    public GameItem(string _itemName, bool isNew = true, int _int = -1, float _float = -1)
    {
        itemFileName = _itemName;
        if (!isNew)
        {
            valInt = _int;
            valFloat = _float;
        }
        else
        {
            //Debug.Log("Creating item " + _itemName + " with values int " + ItemController.instance.items[_itemName].valueInt + " float " + ItemController.instance.items[_itemName].valueFloat);
            valInt = ItemController.instance.items[_itemName].valueInt;
            valFloat = ItemController.instance.items[_itemName].valueFloat;
        }
    }

}

public class ItemController : MonoBehaviour
{
    public static ItemController instance = null;
    public Dictionary<string, Item> items;
    public GameItem[] currentItem;
    public bool[] currentEquip;
    public List<GameItem[]> invs;
    public List<bool[]> equip;
    public SlotController[] slots;
    public GameObject equipBarGo;
    public BarRenderer equipBar;

    public int currdrag;
    public int currhover;
    public int currInv = 0;

    float currLoadingTime, maxLoadingTime;
    int itemLoadingId = -1, itemLoadingEquipId;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        invs = new List<GameItem[]>();
        equip = new List<bool[]>();
        currentItem = new GameItem[10];
        currentEquip = new bool[10];

        invs.Add(currentItem);
        equip.Add(currentEquip);

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].id = i;
        }

        items = new Dictionary<string, Item>();
        Object[] templatesArray;
        templatesArray = Resources.LoadAll("Items/", typeof(Item));
        foreach (Item template in templatesArray)
        {
            //Debug.Log("Template name: " + template.name);
            if (!items.ContainsKey(template.name))
                items.Add(template.name, template);
            /*else
                Debug.Log("Duplicate KEY!");*/
        }
    }

    public void OpenInv()
    {
        currentItem = invs[0];
        currentEquip = equip[0];
        UpdateInv();
    }

    public void CloseInv()
    {
        currInv = 0;
    }

    private void Update()
    {
        if (!GameController.ins.isAlive)
        {
            if (equipBarGo.activeSelf)
            {
                SCP_UI.instance.handEquip.sprite = null;
                SCP_UI.instance.handEquip.SetNativeSize();
                SCP_UI.instance.handEquip.color = Color.clear;
                equipBarGo.SetActive(false);
            }
            return;
        }

        if (itemLoadingId != -1)
        {
            currLoadingTime += Time.deltaTime;
            equipBar.Value = currLoadingTime / maxLoadingTime;
            if (currLoadingTime > maxLoadingTime)
            {
                Use(itemLoadingId, itemLoadingEquipId, true, true);
            }
        }
    }


    public void UpdateInv()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].UpdateInfo();
        }
    }
    public void playerDeath()
    {

    }

    public int AddItem(GameItem item, int inv, bool playSound = true)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (invs[inv][i] == null)
            {
                invs[inv][i] = item;
                Debug.Log(item.itemFileName);
                if (playSound)
                    SCP_UI.instance.ItemSFX(items[item.itemFileName].SFX);

                if (items[item.itemFileName] is Equipable_Wear && ((Equipable_Wear)items[item.itemFileName]).autoEquip)
                {
                    ChangeInv(inv);
                    currInv = inv;
                    currhover = i;
                    slots[i].Use(true);
                }

                return (i);
            }
        }

        return (-1);
    }

    public void Use(int id, int inv, bool isLoaded, bool dontToggle)
    {
        Item currItem = items[invs[inv][id].itemFileName];
        SCP_UI.instance.ItemSFX(currItem.SFX);
        bool dontclose = currItem.keepInv;
        if (currItem.useTime > 0)
        {
            if (!isLoaded)
            {
                StartItemUse(id, inv);
                if (!dontclose && !dontToggle)
                    SCP_UI.instance.ToggleInventory();
                return;
            }
        }
        StopItemUse();
        currItem.Use(ref invs[inv][id]);
        if (currInv == inv && currItem.deleteUse == true)
        {
            if (currItem.isUnique)
                SubtitleEngine.instance.playFormatted("playStrings", "play_used_uni", "itemStrings", currItem.getName());
            else if (currItem.isFem)
                SubtitleEngine.instance.playFormatted("playStrings", "play_used_fem", "itemStrings", currItem.getName());
            else
                SubtitleEngine.instance.playFormatted("playStrings", "play_used_male", "itemStrings", currItem.getName());

            invs[inv][id] = null;
        }
        if (!dontclose && !dontToggle)
            SCP_UI.instance.ToggleInventory();
    }

    void StartItemUse(int itemId, int itemInvId)
    {
        itemLoadingEquipId = itemInvId;
        itemLoadingId = itemId;
        Item currItem = items[invs[itemLoadingEquipId][itemLoadingId].itemFileName];

        currLoadingTime = 0f;
        maxLoadingTime = currItem.useTime;
        equipBarGo.SetActive(true);
        equipBar.Value = 0f;
        SCP_UI.instance.handEquip.sprite = currItem.icon;
        SCP_UI.instance.handEquip.SetNativeSize();
        SCP_UI.instance.handEquip.color = Color.white;
        GameController.ins.currPly.isEquiping = true;
    }

    public void StopItemUse()
    {
        if (itemLoadingId == -1)
            return;
        if (itemLoadingEquipId == -1)
            return;
        Item currItem = items[invs[itemLoadingEquipId][itemLoadingId].itemFileName];
        GameController.ins.currPly.isEquiping = false;
        Debug.Log("Player is equiping false");
        /*if (currItem is Equipable_Wear)
        {
            if (((Equipable_Wear)currItem).autoEquip)
            {
                ItemController.instance.slots[itemLoadingId].UnEquip(true);
            }
        }*/
        itemLoadingEquipId = -1;
        itemLoadingId = -1;
        SCP_UI.instance.handEquip.sprite = null;
        SCP_UI.instance.handEquip.SetNativeSize();
        SCP_UI.instance.handEquip.color = Color.clear;
        equipBarGo.SetActive(false);
        GameController.ins.currPly.Crouch = false;

    }

    public bool IsEmpty(int inv)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (invs[inv][i] != null)
                return false;
        }

        return (true);
    }

    public List<GameItem[]> GetItems()
    {
        return (invs);
    }

    public List<bool[]> GetEquips()
    {
        return (equip);
    }

    public void LoadItems(List<GameItem[]> List, List<bool[]> equips)
    {
        equip = equips;
        invs = List;
    }

    public void SetEquips()
    {
        for (int i = 0; i < invs.Count; i++)
        {
            currInv = i;
            for (int j = 0; j < invs[i].Length; j++)
            {
                currhover = j;
                if (equip[i][j])
                {
                    Item currItem = items[invs[i][j].itemFileName];
                    currItem.Use(ref invs[i][j]);
                }
            }
        }
    }

    public void EmptyItems()
    {
        invs = new List<GameItem[]>();
    }

    public void NewInv()
    {
        //Debug.Log("Creating Inventory");
        invs.Add(new GameItem[10]);
        equip.Add(new bool[10]);
    }

    public void ChangeInv(int inv)
    {
        currentItem = invs[inv];
        currentEquip = equip[inv];
        currInv = inv;
        UpdateInv();
    }
}
