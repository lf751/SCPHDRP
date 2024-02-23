using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatChange : MonoBehaviour
{
    Renderer HeavyLockMat;
    bool lightLock = true;
    float Timer = 0;
    int frame = 0;
    public int globalToCheck = 0;
    public Texture[] frames;
    // Start is called before the first frame update
    void Start()
    {
        HeavyLockMat = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (lightLock == false)
        {
            Timer -= Time.deltaTime;

            if (Timer <= 0)
            {
                if (frame == 1)
                    frame = 0;
                else
                    frame = 1;

                HeavyLockMat.materials[1].SetTexture("_MainTex", frames[frame]);
                Timer = 1;
            }
        }

        if (lightLock != GameController.instance.globalBools[globalToCheck])
        {
            lightLock = GameController.instance.globalBools[globalToCheck];

            if (lightLock == true)
            {
                    HeavyLockMat.materials[1].SetTexture("_MainTex", frames[0]);
            }
        }
    }
}
