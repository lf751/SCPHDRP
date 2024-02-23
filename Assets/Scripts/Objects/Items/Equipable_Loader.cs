using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipable_Loader : Equipable_Wear
{
    public string assetName;
    Object loadedAsset, spawnedAsset;
    // Start is called before the first frame update
    public override void OnEquip(ref gameItem currItem)
    {
        loadedAsset = Resources.Load<GameObject>(string.Concat("Items/Helpers/", assetName));
        spawnedAsset = Instantiate(loadedAsset, SCP_UI.instance.CanvasPos);
    }

    public override void OnDequip(ref gameItem currItem)
    {
        DestroyImmediate(spawnedAsset);
        Resources.UnloadUnusedAssets();
    }
}
