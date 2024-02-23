using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu (fileName = "new Bell", menuName = "Items/Bell")]
public class Bell : Item
{
    public AudioClip[] bellSounds;
    public override void Use(ref gameItem currItem)
    {
        GameController.instance.npcController.simpList[(int)SimpNpcList.bell].isActive=true;
        GameController.instance.PlayHorror(bellSounds[Random.Range(0, bellSounds.Length - 1)], GameController.instance.playercache.transform, npc.none);
    }

}
