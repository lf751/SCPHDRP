using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util_LightBlink : MonoBehaviour
{
    // Start is called before the first frame update
    bool Turned = false;
    float timer = 0;
    Light blinking;

    private void Start()
    {
        blinking = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 1)
        {
            blinking.enabled = !Turned;
            Turned = !Turned;
            timer = 0;
        }
    }
}
