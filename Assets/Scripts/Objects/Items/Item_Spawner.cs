using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_Spawner : MonoBehaviour
{
    [System.Serializable]
    public class itemChance
    {
        public ChanceTable[] items;
    }

    public itemChance [] table;

    public Transform[] positions;
    // Start is called before the first frame update
    public void Spawn()
    {
        for (int i = 0; i < table.Length; i++)
        {
            int gend;
            for (int j = 0; j < table[i].items.Length; j++)
            { 
                gend = Random.Range(0, 100);
                if (gend < table[i].items[j].Rate)
                {
                    GameObject newObject;
                    newObject = Instantiate(GameController.instance.itemSpawner, positions[i].position, positions[i].rotation);
                    Helper(newObject, table[i].items[j].Spawn, positions[i].position);
                    j = table[i].items.Length + 1;
                }
            }
        }
    }

    void Helper(GameObject newObject, Item item, Vector3 position)
    {
        gameItem newItem = new gameItem(item.name);
        newObject.GetComponent<Object_Item>().item = newItem;
        newObject.GetComponent<Object_Item>().id = GameController.instance.AddItem(position, newItem);
        Debug.Log(newObject.GetComponent<Object_Item>().id);
        newObject.GetComponent<Object_Item>().Spawn();
    }
}
