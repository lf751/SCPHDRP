using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipable_Elec_Loader : Equipable_Elec
{
    public string assetName;
    GameObject spawnedAsset;
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
