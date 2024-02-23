using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderWait_Object : MonoBehaviour
{
    public GameObject contains;
    float Timer = GlobalValues.renderTime;

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
        {
            contains.SetActive(false);
            Destroy(this);
        }
    }
}
