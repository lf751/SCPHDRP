using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum WaveFunctions { Sin, Tri, Sqr, Saw, Inv, Noise }

public static class Lib
{


    public static byte GetAnimatorParameterIndex(this Animator anim, string paramName)
    {
        for (byte i = 0; i < anim.parameters.Length; i++)
        {
            if (anim.parameters[i].name == paramName)
            {
                return i;
            }
        }
        Debug.LogError("Parameter " + paramName + " doesn't exist in the animator parameter list!");
        return 0;
    }

    public static bool Contains(this LayerMask layer, int otherLayer)
    {
        return ((layer & (1 << otherLayer)) != 0);
    }

    public static float DegCos(float angle)
    {
        return Mathf.Cos(angle * Mathf.Deg2Rad);
    }

    public static float DegSin(float angle)
    {
        return Mathf.Sin(angle * Mathf.Deg2Rad);
    }

    public static bool GetBit(int number, int bitPosition)
    {
        // Shift the 1 bit to the left by the specified position
        int mask = 1 << bitPosition;

        // Use bitwise AND to check if the bit at the specified position is 1
        // If the result is not 0, the bit is 1; otherwise, it's 0
        return (number & mask) != 0;
    }

    public static int SetBit(int number, int bitPosition, bool newValue)
    {
        // Create a mask by shifting the bit 1 to the left by the specified position
        int mask = 1 << bitPosition;

        if (newValue)
        {
            // If the new value is true, use bitwise OR to set the bit to 1
            return number | mask;
        }
        else
        {
            // If the new value is false, use bitwise AND with the complement of the mask to set the bit to 0
            return number & ~mask;
        }
    }

    public static bool IsInView(Camera cam, Transform who)
    {
        float viewAngle = Camera.VerticalToHorizontalFieldOfView(cam.fieldOfView, cam.aspect) / 2;
        Vector3 objectDir = who.position - cam.transform.position;
        return Vector3.Angle(cam.transform.forward, objectDir.normalized) <= viewAngle;
    }

    public static float SqrDistance(this Vector3 from, Vector3 to)
    {
        return (to - from).sqrMagnitude;
    }

    public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
    {
        return (to - from).normalized;
    }

    public static float EvalWave(WaveFunctions waveFunction, float phase, float frequency, float amplitude)
    {
        float x = (Time.time + phase) * frequency;
        float y;

        x = x - Mathf.Floor(x); // normalized value (0..1)

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

        return (y * amplitude);
    }

}