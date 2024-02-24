using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_DNAButton : Object_Interact
{
    public GameObject Door01, Door02;
    public int handID;
    public AudioClip Accepted, Rejected;
    public AudioSource soundsource;
    public bool WaitForBool;
    public int ThisValue;
    public override void Pressed()
    {
        PlayerControl player = GameController.ins.player.GetComponent<PlayerControl>();

        if (!WaitForBool || (WaitForBool && GameController.ins.globalBools[ThisValue]))
        {
            if (player.equipment[(int)bodyPart.Hand] != null && ItemController.instance.items[player.equipment[(int)bodyPart.Hand].itemFileName] is Equipable_Hand)
            {
                Equipable_Hand hand;
                hand = (Equipable_Hand)ItemController.instance.items[player.equipment[(int)bodyPart.Hand].itemFileName];
                if (hand.handID == handID)
                {
                    Door01.GetComponent<Object_Door>().DoorSwitch();
                    if (Door02 != null)
                        Door02.GetComponent<Object_Door>().DoorSwitch();
                    soundsource.PlayOneShot(Accepted);
                    SubtitleEngine.instance.playSub("playStrings", "play_button_dna");
                }
                else
                {
                    SubtitleEngine.instance.playSub("playStrings", "play_button_wrongdna");
                    soundsource.PlayOneShot(Rejected);
                }

            }
            else
            {
                SubtitleEngine.instance.playSub("playStrings", "play_button_wrongdna");
                soundsource.PlayOneShot(Rejected);
            }
        }
        else
        {
            SubtitleEngine.instance.playSub("playStrings", "play_button_faildna");
            soundsource.PlayOneShot(Rejected);
        }
    }

    public override void Hold()
    {
    }
}

