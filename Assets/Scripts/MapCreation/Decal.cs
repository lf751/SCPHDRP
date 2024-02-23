using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class Decal : MonoBehaviour
{
    public float Duration;
    public bool Instant;
    public float Scale;
    public Vector3 rotation;
    public int h, v;
    public Vector3 position;
    public Material DecalAtlas;
    GameObject plane;
    // Start is called before the first frame update
    void Awake()
    {
        
        plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.transform.position = transform.position;
        plane.transform.parent = transform;

    }

    public void SetDecal()
    {
        transform.position = position;

        plane.transform.rotation = Quaternion.Euler(rotation);

        if (!Instant)
        {
            plane.transform.localScale = Vector3.zero;
            Tween.LocalScale(plane.transform, new Vector3(Scale, Scale, Scale), Duration, 0, Tween.EaseOut);
        }
        else
            transform.localScale = new Vector3(Scale, Scale, Scale);

        Vector2[] uvs;
        Renderer render = plane.GetComponent<Renderer>();
        Destroy(plane.GetComponent<MeshCollider>());
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Mesh mesh = plane.GetComponent<MeshFilter>().mesh;
        uvs = mesh.uv;

        /*
         *UV 0 ESQUINA INFERIOR IZQUIERDA
         *UV 1 ESQUINA SUPERIOR DERECHA
         *UV 2 ESQUINA INFERIOR DERECHA
         *UV 3 ESQUINA SUPERIOR IZQUIERDA
         * */

        float uvH = 0.33f * h;
        float uvV = 1-(0.25f * v);


        uvs[0] = new Vector2(uvH, uvV-0.25f);
        uvs[1] = new Vector2(uvH, uvV );
        uvs[2] = new Vector2(uvH + 0.33f, uvV-0.25f);
        uvs[3] = new Vector2(uvH + 0.33f, uvV);


        mesh.uv = uvs;
        render.material = DecalAtlas;
        render.material = DecalAtlas;
    }
}
