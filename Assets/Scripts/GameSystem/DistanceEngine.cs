using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceEngine : MonoBehaviour
{
    const int defaultSize = 1024;
    public float defRad = 1f, visSize = 45f;
    public static DistanceEngine instance;
    CullingGroup grp;
    BoundingSphere[] spheres;
    Distance_Object[] objects;
    int count = 0;
    int capacity;
    bool cameraAssigned,entitiesRefreshed;
    // Start is called before the first frame update
    void Awake()
    {
        DistanceEngine.instance = this;
        spheres = new BoundingSphere[defaultSize];
        objects = new Distance_Object[defaultSize];
        grp = new CullingGroup();
        grp.onStateChanged += StateChangedMethod;
        grp.SetBoundingDistances(new float[1] {visSize});
        grp.SetBoundingSphereCount(0);
        grp.SetBoundingSpheres(spheres);
        count = 0;
        capacity = defaultSize;
        cameraAssigned = false;
        entitiesRefreshed = false;
    }

    private void StateChangedMethod(CullingGroupEvent evt)
    {
        if (evt.previousDistance != evt.currentDistance || objects[evt.index].firstSpawn)
        {
            objects[evt.index].firstSpawn = false;
            if (evt.currentDistance == 0 && objects[evt.index].Contains!=null)
                objects[evt.index].Contains.SetActive(true);
            else
                objects[evt.index].Contains.SetActive(false);
        }
    }


    private void Update()
    {
        if (GameController.ins.currPly == null || !GameController.ins.isAlive)
        {
            //grp.SetDistanceReferencePoint(new Vector3(0f,-1000f,0f));
            cameraAssigned = false;
            entitiesRefreshed = false;
            return;
        }

        if(cameraAssigned && !entitiesRefreshed)
        {
            entitiesRefreshed = true;
            RefreshEntities();
        }
        grp.SetDistanceReferencePoint(GameController.ins.currPly.transform.position);
        if (!cameraAssigned)
        {
            grp.targetCamera = Camera.main;
            cameraAssigned=true;
        }
        
       
    }

    public int AddToEngine(Distance_Object dist)
    {
        count++;
        if (count >= capacity)
            ExpandArray();
        spheres[count-1] = new BoundingSphere(dist.transform.position, defRad);
        objects[count-1] = dist;
        

        grp.SetBoundingSphereCount(count);
        //Debug.Log("Current count in dengine: " + (count));
        
        return count-1;
    }

    public void DeleteFromEngine(Distance_Object dist)
    {
        if (grp == null)
            return;
        int toDeleteIdx = dist.idx;

        grp.EraseSwapBack(toDeleteIdx);

        --count;
        if (count != 0)
        {
            objects[toDeleteIdx] = objects[count];
            objects[toDeleteIdx].idx = toDeleteIdx;
        }
    }

    void ExpandArray()
    {
        capacity = Mathf.NextPowerOfTwo(count + 1);
        Array.Resize<BoundingSphere>(ref spheres, capacity);
        Array.Resize<Distance_Object>(ref objects, capacity);
        grp.SetBoundingSpheres(spheres);
        Debug.Log("Expanding capacity of "+ name +": " + capacity);
    }

    void RefreshEntities()
    {
        for(int i =0; i < count; i++)
        {
            int thisDistance = grp.GetDistance(i);
            if (thisDistance == 0)
                objects[i].Contains.SetActive(true);
            else
                objects[i].Contains.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        grp.Dispose();
        grp = null;
    }



}
