using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EV_895 : MonoBehaviour
{
    public float perc = 0, percAffectedTime, normalTime, speed;
    public AnimationCurve Curve;
    public Transform Target, Orig;
    Quaternion lookAt;
    public Mesh quad;
    public Material mat;
    public Texture[] text;
    Vector3 moving;
    Camera cam;
    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/
    float Timer = 0;
    bool showing = false;

    private void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer < 0 && showing == false)
        {
            showing = true;
            Timer = 3;
            moving = Orig.position;
            mat.mainTexture = text[Random.Range(0, text.Length - 1)];
        }

        float movePerc = (3-Timer) / 3;

        if (Timer < 0 && showing == true)
        {
            showing = false;
            Timer = normalTime - (percAffectedTime*perc);
        }

        //create the rotation we need to be in to look at the target
        lookAt = Quaternion.LookRotation((Orig.position- Target.position).normalized);

        moving = Vector3.Lerp(moving, Target.position, Curve.Evaluate(movePerc));
        Graphics.DrawMesh(quad, moving, lookAt, mat, 0, null, 0, null, false, false, false);
    }

}
