using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Object_KeyButton : Object_Interact
{
    public GameObject Door01, Door02;
    public int Clearance;
    public AudioClip Accepted, Rejected;
    public AudioSource soundsource;
    public bool WaitForBool;
    public int ThisValue;
    public override void Pressed()
    {
        PlayerControl player = GameController.ins.player.GetComponent<PlayerControl>();


        if (player.equipment[(int)bodyPart.Hand] != null && ItemController.instance.items[player.equipment[(int)bodyPart.Hand].itemFileName] is Equipable_Key)
        {
            if (!WaitForBool || (WaitForBool && GameController.ins.globalBools[ThisValue]))
            {
                Equipable_Key key;
                key = (Equipable_Key)ItemController.instance.items[player.equipment[(int)bodyPart.Hand].itemFileName];
                if (key.level >= Clearance)
                {
                    Door01.GetComponent<Object_Door>().DoorSwitch();
                    if (Door02 != null)
                        Door02.GetComponent<Object_Door>().DoorSwitch();
                    soundsource.PlayOneShot(Accepted);
                    SubtitleEngine.instance.playSub("playStrings", "play_button_card");
                }
                else
                {
                    SubtitleEngine.instance.playSub("playStrings", "play_button_lowcard");
                    soundsource.PlayOneShot(Rejected);
                }
            }
            else
            {
                SubtitleEngine.instance.playSub("playStrings", "play_button_failcard");
                soundsource.PlayOneShot(Rejected);
            }

        }
        else
            SubtitleEngine.instance.playSub("playStrings", "play_button_nocard");

    }

    public override void Hold()
    {
    }
}