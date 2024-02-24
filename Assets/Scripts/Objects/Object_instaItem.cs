using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_instaItem : Object_Interact
{
    public Item itemOG;
    public GameItem item;
    public int id;

    public Mesh itemMesh;
    public Material[] itemMats;
    public BoxCollider col;
    public Rigidbody body;
    // Start is called before the first frame updat
    public void Start()
    {
        //this.transform.parent = GameController.ins.itemParent.transform;
        Spawn();
    }

    public void Spawn()
    {
        item = new GameItem(itemOG.name);
        GameObject model = ItemController.instance.items[item.itemFileName].ItemModel;
        MeshFilter mesh = model.GetComponentInChildren<MeshFilter>(true);
        MeshRenderer renderer = model.GetComponentInChildren<MeshRenderer>(true);
        itemMesh = mesh.sharedMesh;
        itemMats = renderer.sharedMaterials;
        col.center = ItemController.instance.items[item.itemFileName].colCenter;
        col.size = ItemController.instance.items[item.itemFileName].colSize;
        body.mass = ItemController.instance.items[item.itemFileName].mass;
    }

    // Update is called once per frame
    public override void Pressed()
    {
        if (ItemController.instance.AddItem(item, 0) != -1)
        {
            //GameController.ins.DeleteItem(id);
            DestroyImmediate(this.gameObject);

            if (ItemController.instance.items[item.itemFileName].isUnique)
                SubtitleEngine.instance.playFormatted("playStrings", "play_picked_uni", "itemStrings", ItemController.instance.items[item.itemFileName].getName());
            else
            {
                if (ItemController.instance.items[item.itemFileName].isFem)
                    SubtitleEngine.instance.playFormatted("playStrings", "play_picked_fem", "itemStrings", ItemController.instance.items[item.itemFileName].getName());
                else
                    SubtitleEngine.instance.playFormatted("playStrings", "play_picked_male", "itemStrings", ItemController.instance.items[item.itemFileName].getName());
            }
        }
        else
            SubtitleEngine.instance.playSub("playStrings", "play_fullinv");

    }

    public void OnRenderObject()
    {
        if (itemMesh != null)
        {
            //Debug.Log("Mats " + currMat.Length);
            for (int i = 0; i < itemMats.Length; i++)
            {
                Graphics.DrawMesh(itemMesh, gameObject.transform.position, gameObject.transform.rotation, itemMats[i], 0, null, i);
            }
        }
    }

    public override void Hold()
    {
    }

    public void Delete()
    {
        //GameController.ins.DeleteItem(id);
        //DestroyImmediate(this.gameObject);
    }



}
