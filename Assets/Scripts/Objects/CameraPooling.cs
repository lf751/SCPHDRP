using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPooling : MonoBehaviour
{
    public Camera cctv;
    public Renderer screen;
    int renderIndex;
    bool isSwitchOn;
    bool is895;
    public bool isReal895;
    float hackTimer;
    bool was895;

    public void Switch(bool vol)
    {
        isSwitchOn = vol;
        if (!isActiveAndEnabled)
            return;

        if (isSwitchOn)
            OnStartScreen();
        else
            OnStopScreen();
    }

    public void Switch895(bool val)
    {
        is895 = val;
        //Debug.Log("Switcing val: " + val);
        if (!isActiveAndEnabled)
            return;

        OnStopScreen();
        OnStartScreen();
    }

    private void OnEnable()
    {
        //Debug.Log("Starting camera!", this.gameObject);
        if (isSwitchOn)
            OnStartScreen();
    }

    private void OnDisable()
    {
        //Debug.Log("Stoping camera!", this.gameObject);
        OnStopScreen();
        if (is895)
            SCP895Controller.instance.DeSetScreen(screen.gameObject);
    }

    private void Update()
    {
        if (isSwitchOn)
        {
            if (was895 != GameController.ins.globalBools[7])
            {
                Switch895(is895);
                //Debug.Log("895 state change", this.gameObject);
            }

            was895 = GameController.ins.globalBools[7];

            if (is895 && GameController.ins.globalBools[7])
            {
                if ((Time.frameCount % 15 == 0))
                    SCP895Controller.instance.SetClosestScreen(screen.gameObject);
            }

            if (!(is895 && GameController.ins.globalBools[7]) && !isReal895)
            {
                if (hackTimer > 0f)
                {
                    hackTimer -= Time.deltaTime;
                    float flick = Lib.EvalWave(WaveFunctions.Noise, 0, 0.5f, 1);
                    if (flick > 0.5f)
                    {
                        screen.sharedMaterial.SetTexture("_ScreenTex", Scp079Controller.ins.scp079Picture);
                    }
                    else
                    {
                        screen.sharedMaterial.SetTexture("_ScreenTex", cctv.targetTexture);
                    }

                    if (hackTimer < 0)
                        screen.sharedMaterial.SetTexture("_ScreenTex", cctv.targetTexture);
                }
                else
                {
                    bool willHack = false;
                    if (GameController.ins.globalBools[6])
                    {
                        if (Random.value < 0.002f)
                        {
                            willHack = true;
                        }
                    }
                    if (willHack)
                        hackTimer = Random.Range(0.5f, 1.5f);
                }
            }
        }
    }



    void OnStartScreen()
    {
        renderIndex = -1;
        for (int i = 0; i < GameController.ins.cameraPool.Length; i++)
        {
            if (GameController.ins.cameraPool[i].isUsing == false)
            {
                cctv.targetTexture = GameController.ins.cameraPool[i].Renders;
                screen.sharedMaterial = GameController.ins.cameraPool[i].Mats;
                GameController.ins.cameraPool[i].isUsing = true;
                renderIndex = i;
                if (isReal895 && GameController.ins.globalBools[7])
                {
                    //Debug.Log("Starting camera with index " + renderIndex + "is895 + " + isReal895 + "895 is active: " + GameController.ins.globalBools[7], this.gameObject);
                    SCP895Controller.instance.monitorRend.SetTexture("_ScreenTex", cctv.targetTexture);
                }
                //Debug.Log("Tengo camara! " + renderIndex);
                break;
            }
        }
        if ((is895 && GameController.ins.globalBools[7]) || isReal895)
        {
            //Debug.Log("Camera is 895 " + renderIndex, this.gameObject);
            screen.sharedMaterial = SCP895Controller.instance.monitorRend;
        }
    }

    private void OnStopScreen()
    {
        if (renderIndex != -1)
            GameController.ins.cameraPool[renderIndex].isUsing = false;
        //Debug.Log("Stoping camera with index " + renderIndex, this.gameObject);
        if (isReal895 && GameController.ins.globalBools[7])
        {
            //Debug.Log("Camera was 895 " + renderIndex, this.gameObject);
            SCP895Controller.instance.monitorRend.SetTexture("_ScreenTex", SCP895Controller.instance.fakeTexture);
        }
    }


}
