using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 rotation;
    public int h, v;
    public float hsize, vsize;
    public Vector3 position;
    public Material DecalAtlas;
    public bool animate;
    public Vector2Int[] frames;
    public float framerate;
    public float timer;
    int currentframe = 0;
    GameObject plane;
    Mesh mesh;
    Vector2[] uvs;

    void Awake()
    {

        plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.transform.SetPositionAndRotation(transform.position, transform.rotation);
        plane.transform.localScale = transform.localScale;
    }

    void Start()
    {
        plane.transform.parent = transform;
        
        Renderer render = plane.GetComponent<Renderer>();
        Destroy(plane.GetComponent<MeshCollider>());
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mesh = plane.GetComponent<MeshFilter>().mesh;
        uvs = mesh.uv;

        /*
         *UV 0 ESQUINA INFERIOR IZQUIERDA
         *UV 1 ESQUINA SUPERIOR DERECHA
         *UV 2 ESQUINA INFERIOR DERECHA
         *UV 3 ESQUINA SUPERIOR IZQUIERDA
         * */
        if (!animate)
        {
            Frame(h, v);
        }
        else
        {
            Frame(frames[0].x, frames[0].y);
            timer = framerate;
        }
        render.material = DecalAtlas;
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                currentframe += 1;
                if (currentframe >= frames.Length)
                    currentframe = 0;

                Frame(frames[currentframe].x, frames[currentframe].y);
                timer = framerate;
            }
        }
    }

    public void SetFrame(int x, int y)
    {
        Frame(x, y);
    }

    void Frame(int fh, int fv)
    {
        float uvH = hsize * fh;
        float uvV = 1-(vsize * fv);

        uvs[0] = new Vector2(uvH, uvV - vsize);
        uvs[1] = new Vector2(uvH + hsize, uvV - vsize);
        uvs[2] = new Vector2(uvH, uvV);
        uvs[3] = new Vector2(uvH + hsize, uvV);

        mesh.uv = uvs;
    }
}
