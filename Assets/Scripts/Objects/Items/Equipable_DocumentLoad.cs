using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new Document Load", menuName = "Items/DocuLoad")]
public class Equipable_DocumentLoad : Equipable_Wear
{
    public string assetName;
    Object loadedAsset;
    GameObject spawnedAsset;
    public int stringIndex;
    public string filename;

    public override void Use(ref gameItem currItem)
    {
        this.part = bodyPart.Hand;
        base.Use(ref currItem);
    }


    public override void OnEquip(ref gameItem currItem)
    {
        loadedAsset = Resources.Load(string.Concat("Items/Helpers/", assetName));
        spawnedAsset = Instantiate((GameObject)loadedAsset, SCP_UI.instance.CanvasPos);
        this.Overlay = Resources.Load<Sprite>(string.Concat("Items/Docs/", filename));
        spawnedAsset.GetComponent<Text>().text = GameController.instance.globalStrings[stringIndex];
    }

    public override void OnDequip(ref gameItem currItem)
    {
        Resources.UnloadAsset(this.Overlay);
        DestroyImmediate(spawnedAsset);
        Resources.UnloadUnusedAssets();
    }
}

