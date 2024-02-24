using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarRenderer : MonoBehaviour
{
    [SerializeField]
    [Range(0.0f,1.0f)]
    private float fillAmm;
    [SerializeField]
    private float segmentSize;
    [SerializeField]
    private float segFullCount;
    [SerializeField]
    private Image gfx;


    public float Value
    {
        get 
        { 
            return fillAmm; 
        }
        set
        {
            this.fillAmm = value;
            UpdateBar();
        }
    }

    void UpdateBar()
    {
        Debug.Assert(gfx);
        float currSegAmm = (fillAmm * segFullCount);
        //Debug.Log("currSegAmount: " + currSegAmm);
        gfx.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Floor(currSegAmm) * segmentSize);
    }

    private void OnValidate()
    {
        if (!gfx)
            return;

        UpdateBar();
    }
}
