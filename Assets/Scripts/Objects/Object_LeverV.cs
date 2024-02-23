using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;
using Pixelplacement.TweenSystem;

public class Object_LeverV : Object_Interact
{
    public bool On = false;
    public bool OnUp = false;
    bool isUp = false;
    public GameObject Handle;
    public AudioSource sfx;
    public AudioClip Switch;
    public float posUp = -80.399f, posDown = 76.98701f, changeSpeed = 0.6f;

    float changing = 0;
    bool start, change, cooldown;

    public void HandleUpdate(float value)
    {
        Handle.transform.localRotation = Quaternion.Euler(value, 0, 0);
    }

    private void Start()
    {
        if ((!On && !OnUp) || (On && OnUp))
        {
            Tween.Value(posDown, posUp, HandleUpdate, changeSpeed, 0f, Tween.EaseInStrong, Tween.LoopType.None, null);
            isUp = true;
        }
        else
        {
            Tween.Value(posUp, posDown, HandleUpdate, changeSpeed, 0f, Tween.EaseInStrong, Tween.LoopType.None, null);
        }
    }


    // Update is called once per frame
    public override void Hold()
    {
        if (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().y != 0)
        {
            changing -= Time.deltaTime;
            if (changing <= 0)
            {
                if (!cooldown)
                {
                    change = true;
                    start = false;
                }
                else
                {
                    change = false;
                    cooldown = false;
                }
            }

            if (change == true && !cooldown)
            {

                if (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().y > 0)
                {
                    if (!isUp)
                    {
                        sfx.PlayOneShot(Switch);
                        if (OnUp == true)
                            On = true;
                        else
                            On = false;

                        Tween.Value(posDown, posUp, HandleUpdate, changeSpeed, 0f, Tween.EaseInStrong, Tween.LoopType.None, null);
                        Debug.Log("GoingUp");
                        cooldown = true;
                        changing = 0.4f;
                        isUp = true;
                    }
                }
                if (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().y < 0)
                {
                    if (isUp)
                    {
                        sfx.PlayOneShot(Switch);
                        Debug.Log("GoingDown");
                        if (OnUp == true)
                            On = false;
                        else
                            On = true;

                        Tween.Value(posUp, posDown, HandleUpdate, changeSpeed, 0, Tween.EaseInStrong, Tween.LoopType.None, null);
                        cooldown = true;
                        changing = 0.4f;
                        isUp = false;
                    }
                }
            }
        }

        if (start == false)
        {
            changing = 0.3f;
            start = true;
        }


    }
}