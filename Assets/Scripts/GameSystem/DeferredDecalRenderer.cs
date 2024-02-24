using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using System;

//[ExecuteInEditMode]
public class DeferredDecalRenderer : MonoBehaviour
{
	public Mesh m_CubeMesh;
	Camera thisCamera;
	CommandBuffer thisBuf;
	CullingGroup cullGroup;
	CullingGroup cullGroupStat;
	public float renderDistance;
	static MaterialPropertyBlock block;
	int[] resultArray;
	int[] tempArray;
	public const int maxDecals=1024;

	public void OnEnable()
    {
		thisCamera = GetComponent<Camera>();
		thisBuf = new CommandBuffer();
		thisBuf.name = "DeferredDecals";
		thisCamera.AddCommandBuffer(CameraEvent.BeforeReflections, thisBuf);
		cullGroup = new CullingGroup();
		cullGroupStat = new CullingGroup();

		cullGroup.SetBoundingSpheres(DecalSystem.instance.dinSpheres);
		cullGroup.SetBoundingSphereCount(DecalSystem.instance.dinDecalsCount);
		cullGroup.SetBoundingDistances(new float[1] { renderDistance });
		

		cullGroupStat.SetBoundingSpheres(DecalSystem.instance.staSpheres);
		cullGroupStat.SetBoundingSphereCount(DecalSystem.instance.staDecalsCount);
		cullGroupStat.SetBoundingDistances(new float[1] { renderDistance });


		DecalSystem.instance.dinCountUpdate += OnDinSphereCountUpdate;
		DecalSystem.instance.staCountUpdate += OnStaSphereCountUpdate;
		DecalSystem.instance.staSpheresUpdate += OnStaSphereUpdate;
		resultArray = new int[maxDecals];
		tempArray = new int[maxDecals];

		cullGroup.targetCamera = thisCamera;
		cullGroupStat.targetCamera = thisCamera;
	}


	void OnStaSphereCountUpdate()
    {
		cullGroupStat.SetBoundingSphereCount(DecalSystem.instance.staDecalsCount);
	}

	void OnDinSphereCountUpdate()
	{
		cullGroup.SetBoundingSphereCount(DecalSystem.instance.dinDecalsCount);
	}

	void OnStaSphereUpdate()
	{
		cullGroupStat.SetBoundingSpheres(DecalSystem.instance.staSpheres);
	}


	public void OnDisable()
	{
		thisCamera.RemoveCommandBuffer (CameraEvent.BeforeLighting, thisBuf);
		cullGroup.Dispose();
		cullGroup = null;
		cullGroupStat.Dispose();
		cullGroupStat = null;

		if (DecalSystem.instance == null)
			return;
		DecalSystem.instance.dinCountUpdate -= OnDinSphereCountUpdate;
		DecalSystem.instance.staCountUpdate -= OnStaSphereCountUpdate;
		DecalSystem.instance.staSpheresUpdate -= OnStaSphereUpdate;
		
	}

    private void Update()
    {
		cullGroup.SetDistanceReferencePoint(this.transform);
		cullGroupStat.SetDistanceReferencePoint(this.transform);
	}
    private void OnPreRender()
	{
		
        if (DecalSystem.instance == null)
            return;

		var act = gameObject.activeInHierarchy && enabled;
		if (!act)
		{
			OnDisable();
			return;
		}

		//@TODO: in a real system should cull decals, and possibly only
		// recreate the command buffer when something has changed
		thisBuf.Clear();

		// copy g-buffer normals into a temporary RT
		var normalsID = Shader.PropertyToID("_NormalsCopy");
		thisBuf.GetTemporaryRT (normalsID, -1, -1);
		thisBuf.Blit (BuiltinRenderTextureType.GBuffer2, normalsID);

		var specID = Shader.PropertyToID("_SpecCopy");
		thisBuf.GetTemporaryRT(specID, -1, -1);
		thisBuf.Blit(BuiltinRenderTextureType.GBuffer1, specID);

		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}

		int queryRes = cullGroupStat.QueryIndices(true, tempArray, 0);
		Array.Copy(tempArray, resultArray, queryRes);
		int queryRes2 = cullGroup.QueryIndices(true, tempArray, 0);

		Array.Copy(tempArray, 0, resultArray, queryRes, queryRes2);
		
		int currDecals = Mathf.Min(1024, queryRes + queryRes2);

		RenderTargetIdentifier[] mrt = { BuiltinRenderTextureType.GBuffer0, BuiltinRenderTextureType.GBuffer1,
            BuiltinRenderTextureType.GBuffer2, (thisCamera.allowHDR ? BuiltinRenderTextureType.CameraTarget : BuiltinRenderTextureType.GBuffer3) };
		
		thisBuf.SetRenderTarget(mrt, BuiltinRenderTextureType.CameraTarget);
		float colSize = 1.0f / (float)DecalSystem.instance.coluCount;
		float rowSize = 1.0f / (float)DecalSystem.instance.rowCount;

		for (int i = 0; i < currDecals; i++ )
        {	
			GameDecal dec = (i < queryRes ? DecalSystem.instance.staDecals : DecalSystem.instance.dinDecals)[resultArray[i]];
			float invertRow = DecalSystem.instance.rowCount-1 - dec.v;
			//debug += (i < queryRes ? "sta " : "din ") + resultArray[i] + ", pos " + dec.position + ", ";
			block.SetVector("_MainTex_ST", new Vector4(colSize,rowSize,colSize*dec.h,rowSize*invertRow));
			float currScale = Mathf.Lerp(0, dec.scale, dec.startingTime / dec.duration);
            thisBuf.DrawMesh(m_CubeMesh, Matrix4x4.TRS(dec.position,
                dec.rotation,
                new Vector3(currScale, 0.2f, currScale)), DecalSystem.instance.DecalAtlas, 0, 0, block);
        }
		//Debug.Log(debug);

		thisBuf.ReleaseTemporaryRT (normalsID);
		thisBuf.ReleaseTemporaryRT(specID);
	}
	/*
    private void OnDrawGizmos()
    {
        if (DecalSystem.instance == null)
            return;

        Gizmos.color = Color.red;
        
        for (int i = 0; i < DecalSystem.instance.avaiDecals; i++)
        {
            Gizmos.matrix = Matrix4x4.TRS(DecalSystem.instance.DecalPool[i].position,
                Quaternion.Euler(DecalSystem.instance.DecalPool[i].rotation),
                new Vector3(DecalSystem.instance.DecalPool[i].Scale, 0.25f, DecalSystem.instance.DecalPool[i].Scale));
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }*/
}
