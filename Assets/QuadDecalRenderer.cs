using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadDecalRenderer : MonoBehaviour
{
    static MaterialPropertyBlock propBlock;
    static int mainStId;
    // Start is called before the first frame update
    MeshRenderer rend;
    [SerializeField]
    int columns, rows, x, y;
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
        float xSize = 1f / ((float)columns);
        float ySize = 1f / ((float)rows);
        if (propBlock == null) {
            propBlock = new MaterialPropertyBlock();
            mainStId = Shader.PropertyToID("_MainTex_ST");
        }

        propBlock.SetVector(mainStId, new Vector4(xSize, ySize, xSize * x, ySize * y));

        rend.SetPropertyBlock(propBlock);
    }

}
