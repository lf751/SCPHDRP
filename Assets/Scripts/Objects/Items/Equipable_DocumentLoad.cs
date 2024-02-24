using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new Document Load", menuName = "Items/DocuLoad")]
public class Equipable_DocumentLoad : Equipable_Wear
{
    public string assetName;
    GameObject spawnedAsset;
    public int stringIndex;
    public string filename;

    public override void Use(ref GameItem currItem)
    {
        this.part = bodyPart.Hand;
        base.Use(ref currItem);
    }


    public override void OnEquip(ref GameItem currItem)
    {
        spawnedAsset = Instantiate((GameObject)Resources.Load(string.Concat("Items/Helpers/", assetName)));
        this.Overlay = Resources.Load<Sprite>(string.Concat("Items/Docs/", filename));
        spawnedAsset.transform.GetChild(0).GetComponent<Text>().text = GameController.ins.globalStrings[stringIndex];
    }

    public override void OnDequip(ref GameItem currItem)
    {
        Resources.UnloadAsset(this.Overlay);
        DestroyImmediate(spawnedAsset);
        Resources.UnloadUnusedAssets();
    }
}

