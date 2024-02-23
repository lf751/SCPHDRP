using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightFlicker : MonoBehaviour
{
    public enum WaveFunctions {Sin, Tri, Sqr, Saw, Inv, Noise }
        // Properties
    public WaveFunctions waveFunction = WaveFunctions.Sin; // possible values: sin, tri(angle), sqr(square), saw(tooth), inv(verted sawtooth), noise (random)
    public float varbase = 0.0f; // start
    public float amplitude = 1.0f; // amplitude of the wave
    public float phase = 0.0f; // start point inside on wave cycle
    public float frequency = 0.5f; // cycle frequency per second
    private new HDAdditionalLightData light;
    public bool shake = true;
    public Vector3 shakeMult = new Vector3(0.1f,0,0);

    // Keep a copy of the original color
    private float originalIntensity;
    private Vector3 originalPos;

    public HDAdditionalLightData Light { get => light; set => light = value; }

    // Store the original color
    void Start() {
        light = GetComponent<HDAdditionalLightData>();
        originalIntensity = light.intensity;
        originalPos = transform.position;
    }

    void Update()
    {
        light.intensity = originalIntensity * EvalWave();
        transform.position = originalPos + (shakeMult * EvalWave());//new Vector3(x + (EvalWave()*shakeMult), transform.position.y, transform.position.z);
    }

    float EvalWave()
    {
        float x = (Time.time + phase) * frequency;
        float y;

        x = Mathf.Floor(x); // normalized value (0..1)

        switch (waveFunction)
        {
            case WaveFunctions.Sin:
                {
                    y = Mathf.Sin(x * 2 * Mathf.PI);
                    break;
                }
            case WaveFunctions.Tri:
                {
                    if (x < 0.5)
                        y = 4.0f * x - 1.0f;
                    else
                        y = -4.0f * x + 3.0f;
                    break;
                }
            case WaveFunctions.Sqr:
                {
                    if (x < 0.5)
                        y = 1.0f;
                    else
                        y = -1.0f;
                    break;
                }
            case WaveFunctions.Saw:
                {
                    y = x;
                    break;
                }
            case WaveFunctions.Inv:
                {
                    y = 1.0f - x;
                    break;
                }
            case WaveFunctions.Noise:
                {
                    y = 1 - (Random.value * 2);
                    break;
                }
            default:
                {

                    y = 1.0f;
                    break;
                }
        }

        return (y * amplitude) + varbase;
    }
     

}
