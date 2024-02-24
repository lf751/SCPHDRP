using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogChange : MonoBehaviour
{
    bool closeFog = false;
    Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (closeFog)
        {
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, 2, 2 * Time.deltaTime);
            camera.farClipPlane = Mathf.Lerp(camera.farClipPlane, 2, 2 * Time.deltaTime);
        }
        else
        {
            RenderSettings.fogEndDistance = Mathf.Lerp(RenderSettings.fogEndDistance, 15, 2 * Time.deltaTime);
            camera.farClipPlane = Mathf.Lerp(camera.farClipPlane, 15, 2 * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            closeFog = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
            closeFog = true;
    }
}
