using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu (fileName = "new Bell", menuName = "Items/Bell")]
public class Bell : Item
{
    public AudioClip[] bellSounds;
    public override void Use(ref GameItem currItem)
    {
        GameController.ins.npcController.simpList[(int)SimpNpcList.bell].isActive=true;
        GameController.ins.PlayHorror(bellSounds[Random.Range(0, bellSounds.Length - 1)], GameController.ins.currPly.transform, npc.none);
    }

}
