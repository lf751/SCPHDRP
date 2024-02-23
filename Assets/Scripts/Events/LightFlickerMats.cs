using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.HighDefinition;

public class LightFlickerMats : MonoBehaviour
{
    public enum WaveFunctions { Sin, Tri, Sqr, Saw, Inv, Noise }
    public WaveFunctions waveFunction = WaveFunctions.Sin;
    public float varbase = 0.0f;
    public float amplitude = 1.0f;
    public float phase = 0.0f;
    public float frequency = 0.5f;
    [Header("Flicker Settings")]
    public float speed;
    public float noise;
    public Color offColor;
    public Light[] lights;
    public GameObject[] emissiveObjects;
    private bool emissionOff = false;

    private Dictionary<Light, Color> originalColor = new Dictionary<Light, Color>();
    private Dictionary<Light, float> originalIntensity = new Dictionary<Light, float>();
    private Dictionary<GameObject, Color> originalEmissive = new Dictionary<GameObject, Color>();

    void Start()
    {
        foreach (Light light in lights)
        {
            HDAdditionalLightData lightData = light.GetComponent<HDAdditionalLightData>();
            originalIntensity[light] = lightData.intensity;
            originalColor[light] = lightData.color;
        }
        foreach (GameObject emissiveObject in emissiveObjects)
        {
            Renderer emissiveMaterial = emissiveObject.GetComponent<Renderer>();
            originalEmissive[emissiveObject] = emissiveMaterial.material.GetColor("_EmissiveColor");
        }
        StartCoroutine(Flicker());
        StartCoroutine(Intensity());
    }
    IEnumerator Intensity()
    {
        while (true)
        {
            foreach (Light light in lights)
            {
                HDAdditionalLightData lightData = light.GetComponent<HDAdditionalLightData>();
                lightData.color = originalColor[light] * EvalWave();
            }
            foreach (GameObject emissiveObject in emissiveObjects)
            {
                Renderer emissiveMaterial = emissiveObject.GetComponent<Renderer>();
                if (emissionOff)
                {
                    emissiveMaterial.material.SetColor("_EmissiveColor", offColor);
                }
                else
                {
                    emissiveMaterial.material.SetColor("_EmissiveColor", originalEmissive[emissiveObject] * EvalWave());
                }
            }
            yield return null;
        }
    }
    IEnumerator Flicker()
    {
        while (true)
        {
            emissionOff = false;
            foreach (Light light in lights)
            {
                HDAdditionalLightData lightData = light.GetComponent<HDAdditionalLightData>();
                lightData.intensity = originalIntensity[light];
            }

            float randNoise = Random.Range(-1, 1) * Random.Range(-noise, noise);
            yield return new WaitForSeconds(speed + randNoise);

            foreach (Light light in lights)
            {
                HDAdditionalLightData lightData = light.GetComponent<HDAdditionalLightData>();
                lightData.intensity = 0;
            }

            emissionOff = true;
            yield return new WaitForSeconds(speed);
        }
    }

    float EvalWave()
    {
        float x = (Time.time + phase) * frequency;
        float y;

        x = x - Mathf.Floor(x); // normalized value (0..1)

        switch (waveFunction)
        {
            case WaveFunctions.Sin: // sin
                y = Mathf.Sin(x * 2 * Mathf.PI);
                break;
            case WaveFunctions.Tri: // triangle
                if (x < 0.5f)
                    y = 4.0f * x - 1.0f;
                else
                    y = -4.0f * x + 3.0f;
                break;
            case WaveFunctions.Sqr: // square
                if (x < 0.5f)
                    y = 1.0f;
                else
                    y = -1.0f;
                break;
            case WaveFunctions.Saw: // sawtooth
                y = x;
                break;
            case WaveFunctions.Inv: // inverted sawtooth
                y = 1.0f - x;
                break;
            case WaveFunctions.Noise: // random
                y = 1f - (Random.value * 0.25f);
                break;
            default:
                y = 1.0f;
                break;
        }

        return (y * amplitude) + varbase;
    }
}
