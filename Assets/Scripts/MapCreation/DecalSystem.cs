using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable]
public struct GameDecal
{
    public DecalProjector projector;
    public float duration;
    public float startingTime;
    public bool instant;
    public float scale;
    public Vector3 position;
    public Quaternion rotation;
    public int h, v;
}

public class DecalSystem : MonoBehaviour
{
    public const int defDecals = 512;
    public const int defStaDecals = 512;
    public static DecalSystem instance = null;
    public Action staSpheresUpdate, staCountUpdate, dinCountUpdate;
    public Material DecalAtlas;
    public GameDecal[] dinDecals;
    public GameDecal[] staDecals;
    public DecalProjector projectorDecal;
    public int coluCount = 3;
    public int rowCount = 4;
    int dinCount = 0, staCount = 0, staCap, currIdx = 0;
    public int dinDecalsCount { get { return dinCount; } }
    public int staDecalsCount { get { return staCount; } }
    public BoundingSphere[] dinSpheres;
    public BoundingSphere[] staSpheres;
    // Start is called before the first frame update

    private void Awake()
    {
        instance = this;
        //projectorDecal = this.gameObject.AddComponent<DecalProjector>();
        dinDecals = new GameDecal[defDecals];
        staDecals = new GameDecal[defStaDecals];
        dinSpheres = new BoundingSphere[defDecals];
        staSpheres = new BoundingSphere[defStaDecals];
        dinCount = 0;
        staCount = 0;
        staCap = defStaDecals;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Update()
    {
        for (int i = 0; i < dinCount; i++)
        {
            dinDecals[i].startingTime = Mathf.Min(dinDecals[i].duration, dinDecals[i].startingTime += Time.deltaTime);
        }
    }

    public void Decal(Vector3 position, Quaternion rotation, float scale, bool Instant, float Time, int h, int v, bool isPermanent = false)
    {
        if (currIdx >= defDecals)
            currIdx = 0;
        if (currIdx >= dinCount)
        {
            dinDecals[dinCount] = new GameDecal();
            dinSpheres[dinCount] = new BoundingSphere(position, scale);

            dinCount++;
            dinCountUpdate?.Invoke();
        }

        dinSpheres[currIdx].position = position;
        dinSpheres[currIdx].radius = scale;
        dinDecals[currIdx].position = position;
        dinDecals[currIdx].scale = scale;
        dinDecals[currIdx].rotation = rotation;
        dinDecals[currIdx].instant = Instant;
        dinDecals[currIdx].duration = Instant ? 1 : Time;
        dinDecals[currIdx].startingTime = Instant ? 1 : 0;
        dinDecals[currIdx].h = h;
        dinDecals[currIdx].v = v;
        currIdx++;
    }


    public void SpawnDecal(Vector3 here)
    {

        staSpheres[staCount] = new BoundingSphere(here, 2f);
        staDecals[staCount] = new GameDecal();
        staDecals[staCount].scale = 2f;
        staDecals[staCount].rotation = Quaternion.identity;
        staDecals[staCount].instant = true;
        staDecals[staCount].duration = 1f;
        staDecals[staCount].startingTime = 1f;
        staDecals[staCount].h = Random.Range(0, coluCount);
        staDecals[staCount].v = Random.Range(0, rowCount);
        staDecals[staCount].position = here;

        staCount++;
        staCountUpdate?.Invoke();

        if (staCount >= staCap)
            ExpandArray();
    }

    public void DecalStatic(Vector3 position, Quaternion rotation, float scale, int h, int v)
    {

        staSpheres[staCount] = new BoundingSphere(position, scale);
        staDecals[staCount] = new GameDecal();

        staDecals[staCount].scale = scale;
        staDecals[staCount].rotation = rotation;
        staDecals[staCount].instant = true;
        staDecals[staCount].duration = 1f;
        staDecals[staCount].startingTime = 1f;
        staDecals[staCount].h = h;
        staDecals[staCount].v = v;
        staDecals[staCount].position = position;
        staCount++;
        staCountUpdate?.Invoke();

        if (staCount >= staCap)
            ExpandArray();
    }

    void ExpandArray()
    {
        staCap = Mathf.NextPowerOfTwo(staCount + 1);
        Array.Resize<BoundingSphere>(ref staSpheres, staCap);
        Array.Resize<GameDecal>(ref staDecals, staCap);
        staSpheresUpdate?.Invoke();

    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        Gizmos.color = Color.gray;
        for (int i = 0; i < staCount; i++)
        {
            Gizmos.DrawWireCube(staSpheres[i].position, new Vector3(staSpheres[i].radius, 0.1f, staSpheres[i].radius));
        }

        Gizmos.color = Color.blue;

        for (int i = 0; i < dinCount; i++)
        {
            float currScale = Mathf.Lerp(0, dinDecals[i].scale, dinDecals[i].startingTime / dinDecals[i].duration);
            Gizmos.DrawWireCube(dinSpheres[i].position, new Vector3(currScale, 0.2f, currScale));
        }

    }

}
