using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPooling : MonoBehaviour
{
    public Camera cctv;
    public Renderer screen;
    int renderIndex;
 
    void OnEnable()
    {
        renderIndex = -1;
        for(int i = 0; i < GameController.instance.cameraPool.Length; i++)
        {
            if (GameController.instance.cameraPool[i].isUsing == false)
            {
                cctv.targetTexture = GameController.instance.cameraPool[i].Renders;
                screen.material = GameController.instance.cameraPool[i].Mats;
                GameController.instance.cameraPool[i].isUsing = true;
                renderIndex = i;
                //Debug.Log("Tengo camara! " + renderIndex);
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (renderIndex != -1)
            GameController.instance.cameraPool[renderIndex].isUsing = false;
    }


}
