using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameDecal
{
    public enum Kind
    {
        OnlyPBR, OnlyNormals, PBRNormals, Plane
    }
    public Kind decalType;
    public float Duration;
    public float time;
    public bool Instant;
    public float Scale;
    public Vector3 rotation;
    public int h, v;
    public Vector3 position;
    public bool isPermanent;
}




public class DecalSystem : MonoBehaviour
{
    public const int maxDecals = 1024;
    public static DecalSystem instance = null;

    public Material DecalAtlas;
    public GameDecal[] DecalPool;
    int currDecal = 0;
    int currMaxDecals = 0;
    public int avaiDecals { get { return currMaxDecals; } }
    BoundingSphere[] spheres;
    CullingGroup decalsGroup;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
    }

    void Start()
    {
        DecalPool = new GameDecal[maxDecals];
        decalsGroup = new CullingGroup();
        spheres = new BoundingSphere[maxDecals];
        decalsGroup.SetBoundingSpheres(spheres);
        decalsGroup.SetBoundingSphereCount(0);
        decalsGroup.targetCamera = Camera.main;
        currMaxDecals = 0;
        currDecal = 0;

        /*for (int j = 0; j < 10; j++)
        {
            SpawnDecal(new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.7f * j));
        }
        CombineDecals();


        Decal(new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.7f), new Vector3(90f, 0, 0), 5f, false, 6f, 2, 0);
        Decal(new Vector3(transform.position.x, transform.position.y, transform.position.z - 1.7f), new Vector3(90f, 0, 0), 1f, true, 6f, 0, 3);*/
    }

    public void Decal(Vector3 position, Vector3 rotation, float scale, bool Instant, float Time, int h, int v, bool isPermanent = false)
    {

        while (DecalPool[currDecal] != null && DecalPool[currDecal].isPermanent)
        {
            currDecal++;
        }

        if(DecalPool[currDecal] == null)
            DecalPool[currDecal] = new GameDecal();

        spheres[currDecal] = new BoundingSphere(position, scale);
        DecalPool[currDecal].Scale = scale;
        DecalPool[currDecal].rotation = rotation;
        DecalPool[currDecal].Instant = Instant;
        DecalPool[currDecal].Duration = Time;
        DecalPool[currDecal].h = h;
        DecalPool[currDecal].v = v;
        DecalPool[currDecal].position = position;
        DecalPool[currDecal].isPermanent = isPermanent;

        currDecal++;

        if (currDecal > currMaxDecals)
        {
            currMaxDecals = currDecal;
            decalsGroup.SetBoundingSphereCount(currMaxDecals);
        }

        if (currDecal == maxDecals)
            currDecal = 0;
    }

    private void OnDisable()
    {
        decalsGroup.Dispose();
        decalsGroup = null;
    }

    public void SpawnDecal(Vector3 here)
    {
        /*Vector2[] uvs;
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Quad);
        plane.transform.position = here;
        plane.transform.parent = transform;
        plane.transform.rotation = Quaternion.Euler(90.0f, 0, 0);
        Renderer render = plane.GetComponent<Renderer>();
        render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        Mesh mesh = plane.GetComponent<MeshFilter>().mesh;
        Destroy(plane.GetComponent<MeshCollider>());
        uvs = mesh.uv;
        plane.transform.localScale = new Vector3(2f, 2f, 2f);

        /*
         *UV 0 ESQUINA INFERIOR IZQUIERDA
         *UV 1 ESQUINA SUPERIOR DERECHA
         *UV 2 ESQUINA INFERIOR DERECHA
         *UV 3 ESQUINA SUPERIOR IZQUIERDA
         * */
         /*
        float uvH = 0.33f * (Random.Range(0, 4));
        float uvV = 0.25f * (Random.Range(0, 5));

        uvs[0] = new Vector2(uvH, uvV- 0.25f);
        uvs[1] = new Vector2(uvH, uvV);
        uvs[2] = new Vector2(uvH + 0.33f, uvV- 0.25f);
        uvs[3] = new Vector2(uvH + 0.33f, uvV);

        mesh.uv = uvs;
        render.material = DecalAtlas;*/
    }

}
