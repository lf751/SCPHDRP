using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class ChanceTable
{
    public Item Spawn;
    public int Rate;
}

public class SCP_914 : MonoBehaviour
{
    public GameObject Knob, spawner;
    public Object_914Dial dial;
    public Object_914Key key;
    public Transform intake, outake;
    public Object_Door door1, door2;
    public LayerMask items;
    public AudioClip refine;

    Collider[] inItems;

    bool Activated;
    float Refining;



    [System.Serializable]
    public class Table914
    {
        public Item Original;
        public ChanceTable[] _Coarse;
        public ChanceTable[] _Rough;
        public ChanceTable[] _11;
        public ChanceTable[] _Fine;
        public ChanceTable[] _VeryFine;
    }

    public Table914[] itemtable;

    public Table914 document;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        switch (dial.Option)
        {
            case 4:
                {
                    Knob.transform.localRotation = Quaternion.Euler(0, 180, -83f);
                    break;
                }
            case 3:
                {
                    Knob.transform.localRotation = Quaternion.Euler(0, 180, -43.5f);
                    break;
                }
            case 2:
                {
                    Knob.transform.localRotation = Quaternion.Euler(0, 180, 0f);
                    break;
                }
            case 1:
                {
                    Knob.transform.localRotation = Quaternion.Euler(0, 180, 44f);
                    break;
                }
            case 0:
                {
                    Knob.transform.localRotation = Quaternion.Euler(0, 180, 83f);
                    break;
                }
        }
        if (key.Activated == true && Activated == false && Refining <= 0)
        {
            Activated = true;
            GameController.ins.GlobalSFX.PlayOneShot(refine);
            door1.DoorSwitch();
            door2.DoorSwitch();
            Refining = 13f;
        }

        if (Activated == true)
        {

            if (Refining <= 1)
            {
                Refine();
                door1.DoorSwitch();
                door2.DoorSwitch();
                Activated = false;
            }

        }
        Refining -= Time.deltaTime;




    }

    public void Refine()
    {
        inItems = Physics.OverlapSphere(intake.transform.position, 1.5f, items);

        if (inItems.Length != 0)
        {
            for (int i = 0; i < inItems.Length; i++)
            {
                GameObject newItem;
                newItem = Instantiate(spawner, outake.position, Quaternion.identity);
                newItem.GetComponent<Object_Item>().item = TransformItem(ItemController.instance.items[inItems[i].GetComponent<Object_Item>().item.itemFileName]);
                newItem.GetComponent<Object_Item>().Spawn();
                inItems[i].GetComponent<Object_Item>().Delete();

            }
        }

    }



    public GameItem TransformItem(Item item)
    {
        if (item is Document_Equipable)
            return (new GameItem(getItem(document, item).name));

        for (int i = 0; i < itemtable.Length; i++)
        {
            if (item.name == itemtable[i].Original.name)
            {
                return (new GameItem(getItem(itemtable[i], item).name));
            }



        }
        return (new GameItem(item.name));
    }



    Item getItem(Table914 table, Item item)
    {
        int gend;

        switch (dial.Option)
        {
            case 0:
                {
                    for (int j = 0; j < table._Coarse.Length; j++)
                    {
                        gend = Random.Range(0, 100);
                        if (gend < table._Coarse[j].Rate)
                        {
                            return (table._Coarse[j].Spawn);
                        }
                    }
                    break;
                }
            case 1:
                {
                    for (int j = 0; j < table._Rough.Length; j++)
                    {
                        gend = Random.Range(0, 100);
                        if (gend < table._Rough[j].Rate)
                        {
                            return (table._Rough[j].Spawn);
                        }
                    }
                    break;
                }
            case 2:
                {
                    for (int j = 0; j < table._11.Length; j++)
                    {
                        gend = Random.Range(0, 100);
                        if (gend < table._11[j].Rate)
                        {
                            return (table._11[j].Spawn);
                        }
                    }
                    break;
                }
            case 3:
                {
                    for (int j = 0; j < table._Fine.Length; j++)
                    {
                        gend = Random.Range(0, 100);
                        if (gend < table._Fine[j].Rate)
                        {
                            return (table._Fine[j].Spawn);
                        }
                    }
                    break;
                }
            case 4:
                {
                    for (int j = 0; j < table._VeryFine.Length; j++)
                    {
                        gend = Random.Range(0, 100);
                        if (gend < table._VeryFine[j].Rate)
                        {
                            return (table._VeryFine[j].Spawn);
                        }
                    }
                    break;
                }

        }



        return (item);
    }
}
