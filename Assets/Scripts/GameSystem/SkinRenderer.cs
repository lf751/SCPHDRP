using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SkinRenderer : MonoBehaviour
{
    public Texture2D skinLookup;
    
    // Start is called before the first frame update
    public void OnEnable()
    {
        Shader.SetGlobalTexture("_GlobalSkinTone", skinLookup);
    }
    public void OnStart()
    {
        Shader.SetGlobalTexture("_GlobalSkinTone", skinLookup);
    }
}
