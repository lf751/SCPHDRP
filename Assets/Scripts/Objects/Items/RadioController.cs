using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadioController : MonoBehaviour
{
    public AudioClip[] station3, station4, station5, eventBasedAudio;
    public AudioClip station2, sndstatic, change, station3_music;
    public Image[] bats;
    float[] ch_Timer, ch_Seek;
    int[] ch_State;
    int chn=1;

    public GameObject radio, display;
    public bool isActive;
    bool isPlay = false, playedTuto;
    public AudioSource sfxStatic, sfxRadio;

    public Text channel, broadcast;
    public RectTransform displaysize, displaytext;
    public float dotTimer;

    Equipable_Radio _Radio;
    gameItem currRadio;
    // Start is called before the first frame update
    void Start()
    {
        ch_Timer = new float[5] { -1, -1, -1, -1, -1 };
        ch_Seek = new float[5] { -1, -1, -1, -1, -1 };
        ch_State = new int[5] { 0, 0, 0, 0, 0 };

        StopRadio();

    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            if (_Radio.SpendBattery)
            {
                if (currRadio.valFloat > 0)
                {
                    int batPercent = (Mathf.CeilToInt((5 * currRadio.valFloat) / 100));

                    for (int i = 0; i < 4; i++)
                    {
                        batPercent -= 1;
                        if (batPercent > 0)
                            bats[i].fillCenter = true;
                        else
                            bats[i].fillCenter = false;
                    }
                }
                else
                    DimRadio();
            }

            dotTimer -= Time.deltaTime;
            if (dotTimer <= 0)
            {
                dotTimer = 0.5f;


                displaytext.anchoredPosition = new Vector2(displaytext.anchoredPosition.x - 5, displaytext.anchoredPosition.y);
                if (displaytext.anchoredPosition.x < (0 - ((broadcast.preferredWidth/4) + displaysize.sizeDelta.x)))
                    displaytext.anchoredPosition = new Vector2(0, displaytext.anchoredPosition.y);
            }



            if (SCPInput.instance.playerInput.Gameplay.Radio1.triggered)
                chgChn(1);
            if (SCPInput.instance.playerInput.Gameplay.Radio2.triggered)
                chgChn(2);
            if (SCPInput.instance.playerInput.Gameplay.Radio3.triggered)
                chgChn(3);
            if (SCPInput.instance.playerInput.Gameplay.Radio4.triggered)
                chgChn(4);
            if (SCPInput.instance.playerInput.Gameplay.Radio5.triggered)
                chgChn(5);
        }



        switch (chn)
        {
            case 1:
                {
                    break;
                }
            case 2:
                {
                    break;
                }

            default:
                {
                    ch_Timer[chn - 1] -= Time.deltaTime;
                    if (isPlay)
                        ch_Seek[chn - 1] = sfxRadio.time;
                    else
                        ch_Seek[chn - 1] = -1;

                    if (ch_Timer[chn - 1] <= 0)
                    {
                        if (isPlay == false)
                        {
                            if (isActive && ch_State[chn - 1] != -1)
                            {
                                sfxRadio.Stop();
                                sfxRadio.time = 0f;
                                isPlay = true;
                                float Timer = 0;
                                AudioClip play = sndstatic;

                                switch (chn)
                                {
                                    case 3:
                                        {
                                            if (ch_State[chn - 1] != station3.Length)
                                            {
                                                Timer = station3[ch_State[chn - 1]].length + 1;
                                                play = station3[ch_State[chn - 1]];
                                            }
                                            else
                                                ch_State[chn - 1] = -1;
                                            break;
                                        }
                                    case 4:
                                        {
                                            if (ch_State[chn - 1] != station4.Length)
                                            {
                                                Timer = station4[ch_State[chn - 1]].length + 1;
                                                play = station4[ch_State[chn - 1]];
                                            }
                                            else
                                                ch_State[chn - 1] = -1;
                                            break;
                                        }
                                    case 5:
                                        {
                                            if (ch_State[chn - 1] != station5.Length)
                                            {
                                                Timer = station5[ch_State[chn - 1]].length + 1;
                                                play = station5[ch_State[chn - 1]];
                                            }
                                            else
                                                ch_State[chn - 1] = -1;
                                        break;
                                        }
                                }
                                ch_Timer[chn - 1] = Timer;
                                sfxRadio.clip = play;
                                sfxRadio.Play();
                            }
                        }
                        else 
                        {
                            isPlay = false;
                            ch_Timer[chn - 1] = Random.Range(3, 7);
                            ch_State[chn - 1] += 1;
                            sfxRadio.Stop();

                            if (chn == 3)
                            {
                                sfxRadio.clip = station3_music;
                                ch_Timer[chn - 1] = station3_music.length;
                                sfxRadio.Play();
                            }


                        }

                    }
                    break;
                }
        }

        /*if (Input.GetKeyDown(KeyCode.Alpha9))
            StartRadio();
        if (Input.GetKeyDown(KeyCode.Alpha0))
            StopRadio();*/
    }

    public void chgChn(int channelno)
    {
        displaytext.anchoredPosition = new Vector2(0, displaytext.anchoredPosition.y);
        chn = channelno;
        channel.text = chn.ToString();

        sfxRadio.Stop();

        switch (chn)
        {
            case 1:
                {
                    broadcast.text = "";
                    
                    break;
                }
            case 2:
                {
                    broadcast.text = Localization.GetString("uiStrings", "ui_radio_channel2");
                    break;
                }

            default:
                {
                    switch (chn)
                    {
                        case 3:
                            {
                                broadcast.text = Localization.GetString("uiStrings", "ui_radio_channel3");
                                break;
                            }
                        case 4:
                            {
                                broadcast.text = Localization.GetString("uiStrings", "ui_radio_channel4");
                                break;
                            }
                        case 5:
                            {
                                broadcast.text = "";
                                break;
                            }
                    }

                    if (ch_Seek[chn - 1] > 0)
                    {
                        isPlay = true;
                        AudioClip play = sndstatic;

                        switch (chn)
                        {
                            case 3:
                                {
                                    broadcast.text = Localization.GetString("uiStrings", "ui_radio_channel3");
                                    play = station3[ch_State[chn - 1]];
                                    break;
                                }
                            case 4:
                                {
                                    broadcast.text = Localization.GetString("uiStrings", "ui_radio_channel4");
                                    play = station4[ch_State[chn - 1]];
                                    break;
                                }
                            case 5:
                                {
                                    play = station5[ch_State[chn - 1]];
                                    break;
                                }
                        }

                        sfxRadio.clip = play;
                        sfxRadio.time = ch_Seek[chn - 1];
                        sfxRadio.Play();
                    }
                    else
                        isPlay = false;

                    break;
                }
        }
        sfxStatic.PlayOneShot(change);
    }

    public void StartRadio()
    {
        /*sfxStatic.volume = 0.2f;
        sfxRadio.volume = 1f;
        isActive = true;*/
        
        radio.SetActive(true);
        TurnRadio();
        currRadio = GameController.instance.player.GetComponent<Player_Control>().equipment[(int)bodyPart.Hand];
        _Radio = ((Equipable_Radio)ItemController.instance.items[currRadio.itemFileName]);
        sfxStatic.PlayOneShot(change);
    }

    public void DimRadio()
    {
        /*if (_Radio.valueFloat < 0 && _Radio.SpendBattery)
            display.SetActive(false);
        else
            display.SetActive(true);*/

        display.SetActive(false);
        sfxStatic.volume = 0f;
        sfxRadio.volume = 0f;
    }

    public void TurnRadio()
    {
        display.SetActive(true);
        sfxStatic.volume = 0.2f;
        sfxRadio.volume = 1f;
        isActive = true;
        if (!playedTuto)
        {
            playedTuto = true;
            SCP_UI.instance.ShowTutorial("tutoradio");
        }
    }



    public void StopRadio()
    {
        sfxStatic.volume = 0f;
        sfxRadio.volume = 0f;
        isActive = false;
        radio.SetActive(false);
    }

}
