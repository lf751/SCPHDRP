using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipable_Loader : Equipable_Wear
{
    public string assetName;
    GameObject spawnedAsset;
    // Start is called before the first frame update
    public override void OnEquip(ref GameItem currItem)
    {
        spawnedAsset = Instantiate(Resources.Load<GameObject>(string.Concat("Items/Helpers/", assetName)));
    }

    public override void OnDequip(ref GameItem currItem)
    {
        DestroyImmediate(spawnedAsset);
        Resources.UnloadUnusedAssets();
    }
}
