using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimit : MonoBehaviour
{
    Camera cam;
    // Start is called before the first frame update
    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        cam.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 15 == 0)
            cam.Render();
    }
}
