using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubemapLoader : MonoBehaviour
{
    public Cubemap cube;
    public Cubemap currCube;
    // Start is called before the first frame update
    void Start()
    {
        if (transform.rotation.eulerAngles.y == 90)
        {
            currCube = new Cubemap(cube.height, cube.format, true);
        }
        for(int i = 0; i < cube.mipmapCount; i++)
        {
            currCube.SetPixels(cube.GetPixels(CubemapFace.PositiveX, i), CubemapFace.PositiveZ, i);
            currCube.SetPixels(cube.GetPixels(CubemapFace.PositiveZ, i), CubemapFace.NegativeX, i);
            currCube.SetPixels(cube.GetPixels(CubemapFace.NegativeX, i), CubemapFace.NegativeZ, i);
            currCube.SetPixels(cube.GetPixels(CubemapFace.NegativeZ, i), CubemapFace.PositiveX, i);
        }
        currCube.Apply();
        GetComponent<ReflectionProbe>().customBakedTexture = currCube;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
