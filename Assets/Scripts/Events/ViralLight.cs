using UnityEngine;
using System.Collections;

public class ViralLight : MonoBehaviour
{
    public enum lightType { flicker, pulsate }
    public lightType type = lightType.flicker;

    public Light light;
    public float speed;
    public float noise;
    // Use this for initialization
    void Start()
    {
        light.enabled = false; //Initially turn off Light
        if (type == lightType.flicker)
        {
            StartCoroutine(Flicker());
        }
        else if (type == lightType.pulsate)
        {

        }

    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator Flicker()
    {
        light.enabled = true;
        float randNoise = Random.Range(-1, 1) * Random.Range(-noise, noise);
        yield return new WaitForSeconds(speed + randNoise);
        light.enabled = false;
        yield return new WaitForSeconds(speed);
        StartCoroutine(Flicker());
    }

}