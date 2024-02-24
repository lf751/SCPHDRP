using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    public int numberOfPoints = 10;
    public float radius = 5f;
    public LineRenderer line;
    Vector3[] points;
    void Awake()
    {
        if (!line)
            line = GetComponent<LineRenderer>();
        points = new Vector3[numberOfPoints];
    }

    void GenerateCirclePoints()
    {
        for (int i = 0; i < numberOfPoints; i++)
        {
            float angle = (float)i / numberOfPoints * 2 * Mathf.PI;
            points[i].x = Mathf.Cos(angle) * radius;
            points[i].y = Mathf.Sin(angle) * radius;
        }

        line.positionCount = numberOfPoints;
        line.SetPositions(points);
    }

    public void UpdateRadius(float rad)
    {
        radius = rad;
        GenerateCirclePoints();
    }
    void OnValidate()
    {
        if (!line)
            return;
        points = new Vector3[numberOfPoints];
        GenerateCirclePoints();
    }


}
