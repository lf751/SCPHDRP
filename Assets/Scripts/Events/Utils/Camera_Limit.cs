using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Limit : MonoBehaviour
{
    private new Camera renderer;
    // Start is called before the first frame update
    private void Awake()
    {
        renderer = GetComponent<Camera>();
    }

    void Start()
    {
        renderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 15 == 0)
            renderer.Render();
    }
}
