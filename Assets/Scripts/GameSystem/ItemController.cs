using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class gameItem
{
    public string itemFileName;
    public int valInt=-1;
    public float valFloat=-1;

    public gameItem(string _itemName, bool isNew = true, int _int = -1, float _float = -1)
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
    public gameItem [] currentItem;
    public bool[] currentEquip;
    public List<gameItem[]> invs;
    public List<bool []> equip;
    public slotController [] slots;

    public int currdrag;
    public int currhover;
    public int currInv=0;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        invs = new List<gameItem[]>();
        equip = new List<bool[]>();
        currentItem = new gameItem[10];
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
        foreach (Item template in templatesArray.Cast<Item>())
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


    
    public void UpdateInv()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].updateInfo();
        }
    }
    public void playerDeath()
    {

    }

    public int AddItem(gameItem item, int inv)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (invs[inv][i] == null)
            {
                invs[inv][i] = item;
                Debug.Log(item.itemFileName);
                SCP_UI.instance.ItemSFX(items[item.itemFileName].SFX);

                if (items[item.itemFileName] is Equipable_Wear wear && wear.autoEquip)
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

    public bool IsEmpty(int inv)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (invs[inv][i] != null)
                return false;
        }

        return (true);
    }

    public List<gameItem[]> GetItems()
    {
        return (invs);
    }

    public List<bool[]> GetEquips()
    {
        return (equip);
    }

    public void LoadItems(List<gameItem[]> List, List<bool[]> equips)
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
        invs = new List<gameItem[]>();
    }

    public void NewInv()
    {
        //Debug.Log("Creating Inventory");
        invs.Add(new gameItem[10]);
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
