using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        int rectLeft = Screen.width / 2;
        // Make a background box
        GUI.Box(new Rect(rectLeft, 10, 500, 120), "Menu Fade");

        if (GUI.Button(new Rect(rectLeft, 40, 80, 20), "FadeOut"))
        {
            LoadingSystem.instance.FadeOut(1f, new Vector3Int(0, 0, 0));


        }
        if (GUI.Button(new Rect(rectLeft, 85, 80, 20), "Fadeinr"))
        {
            LoadingSystem.instance.FadeIn(1f, new Vector3Int(255, 255, 255));
        }

    }
}
