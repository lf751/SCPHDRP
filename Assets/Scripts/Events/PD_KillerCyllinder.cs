using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class PD_KillerCyllinder : MonoBehaviour
{
    public Spline ThisSpline;
    public float duration;
    // Start is called before the first frame update
    void Start()
    {
        Tween.Spline(ThisSpline, transform, 0, 1, false,duration, 0, Tween.EaseLinear, Tween.LoopType.Loop);
    }
}
