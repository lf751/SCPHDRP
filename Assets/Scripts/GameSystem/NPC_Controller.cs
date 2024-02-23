using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum npc { scp173, scp106, scp096, scp049, none };
public enum npctype { guard, sci, scp939, zombie, npc };
public enum npcstate { alive, death };

public class NPC_Controller : MonoBehaviour
{
    

    public GameObject[] NPC_Prefabs;
    public GameObject[] Simp_Prefabs;
    public GameObject[] SCP_Prefabs;

    [HideInInspector]
    [System.NonSerialized]
    public Roam_NPC[] mainList = new Roam_NPC[4];
    [System.NonSerialized]
    public SimpleNPC[] simpList = new SimpleNPC[1];
    [HideInInspector]
    [System.NonSerialized]
    public List<Map_NPC> NPCS = new();

    GameObject parent;

    /// <summary>
    /// NPC Data
    /// </summary>
    int place173_curr = 0;
    bool npcPanel = false;
    public Texture npcCamText;
    npc DebugNPC;
    int debugX;
    int debugY;

    public bool[] spawnTable;
    public Transform[] spawnPos;

    public bool[] spawnSimps;


    npc LatestNPC = npc.none;
    npc ZoneMain = npc.none;

    public npc Zone3_Main;
    public npc Zone2_Main;
    public npc Zone1_Main;

    float NPCTimer;



    // Start is called before the first frame update
    void Awake()
    {
        parent = new GameObject("npcParent");
    }

    public void DeleteNPC()
    {
        NPCS.Clear();
        DestroyImmediate(parent);
        parent = new GameObject("npcParent");
    }

    public void ResetNPC(NPC_Data[] npc, NPC_Data[] scp, bool[]simps)
    {
        for(int i=0; i < npc.Length; i++)
        {
            AddNpc(npc[i].type, npc[i].Pos.toVector3(), npc[i]);
        }

        for (int i = 0; i < simpList.Length; i++)
        {
            simpList[i].isActive = simps[i];
            //Debug.Log("Obteniendo datos NPC " + i + " de " + NPCS.Count + " tipo " + NPCS[i].data.type);
        }

        for (int v = 0; v < scp.Length; v++)
        {
            if (spawnTable[v] == true)
            {
                Debug.Log("Enemigo " + v + " pos " + scp[v].Pos.toVector3() + " Activo? " + scp[v].isActive);
                mainList[v].Spawn(scp[v].isActive, scp[v].Pos.toVector3());
                mainList[v].setData(scp[v]);
                //mainList[v].transform.rotation = Quaternion.Euler(scp[v].Rotation.toVector3());
            }
        }
    }

    public int AddNpc(npctype newType, Vector3 where, NPC_Data data = null)
    {
        

        GameObject newNPC = Instantiate(NPC_Prefabs[(int)newType], where, Quaternion.identity, parent.transform);
        if (data == null)
        {
            newNPC.GetComponent<Map_NPC>().createData();
            Debug.Log("Creando NPC tipo " + newType + " con ID " + (NPCS.Count));
        }
        else
        {
            newNPC.GetComponent<Map_NPC>().setData(data);
            Debug.Log("Reposicionando NPC tipo " + newType + " con ID " + (NPCS.Count));
        }
        NPCS.Add(newNPC.GetComponent<Map_NPC>());
        
        return NPCS.Count-1;
    }

    public void npcLevel(npc who)
    {
        Debug.Log("Settint Level for scp " + who + " Latest being " + LatestNPC + " current Main " + ZoneMain);
        if (LatestNPC != ZoneMain && who != ZoneMain)
        {
            if (LatestNPC != npc.none)
                mainList[(int)LatestNPC].SetAgroLevel(0);
            mainList[(int)who].SetAgroLevel(1);
            LatestNPC = who;
            NPCTimer = 75;
        }

        if (LatestNPC != npc.none && who == ZoneMain)
        {
            mainList[(int)LatestNPC].SetAgroLevel(0);
            mainList[(int)who].SetAgroLevel(1);
            LatestNPC = who;
            NPCTimer = 90;
        }

        if (who == ZoneMain)
        {
            mainList[(int)who].SetAgroLevel(1);
            LatestNPC = who;
            NPCTimer = 120;
        }


    }

    public void NPCManager()
    {
        NPCTimer -= Time.deltaTime;

        if (NPCTimer <= 0)
        {
            LatestNPC = npc.none;
        }

        if (GameController.instance.currZone == 2 && ZoneMain != Zone3_Main)
        {
            SetMainNPC(Zone3_Main);
        }
        if (GameController.instance.currZone == 1 && ZoneMain != Zone2_Main)
        {
            SetMainNPC(Zone2_Main);
        }
        if (GameController.instance.currZone == 0 && ZoneMain != Zone1_Main)
        {
            SetMainNPC(Zone1_Main);
        }
    }

    void SetMainNPC(npc New)
    {
        for (int i = 0; i < mainList.Length; i++)
        {
            mainList[i].SetAgroLevel(0);
        }
        mainList[(int)New].SetAgroLevel(1);
        ZoneMain = New;
    }

    public NPC_Data[] getData()
    {
        NPC_Data[] helper = new NPC_Data[NPCS.Count];
        for (int i=0; i < NPCS.Count;i++)
        {
            helper[i]= NPCS[i].getData();
            Debug.Log("Obteniendo datos NPC " + i + " de " + NPCS.Count + " tipo " + NPCS[i].data.type);
        }
        return (helper);
    }

    public bool[] getActiveSimps()
    {
        Debug.Log("Simp size = " + Simp_Prefabs.Length);
        bool[] helper = new bool[Simp_Prefabs.Length];
        for (int i = 0; i < Simp_Prefabs.Length; i++)
        {
            helper[i] = simpList[i].isActive;
            //Debug.Log("Obteniendo datos NPC " + i + " de " + NPCS.Count + " tipo " + NPCS[i].data.type);
        }
        return (helper);
    }

    public NPC_Data[] getMain()
    {
        NPC_Data[] helper = new NPC_Data[mainList.Length];
        for (int i = 0; i < mainList.Length; i++)
        {
            if (spawnTable[i] == true)
            {
                helper[i] = mainList[i].getData();
            }
        }
        return (helper);
    }


    public void GL_Spawn()
    {
        for(int i=0; i<spawnTable.Length;i++)
        {
            if (spawnTable[i] == true)
            {
                Debug.Log("Spawning " + i + " name " + SCP_Prefabs[i].name);
                GameObject newSCP = Instantiate(SCP_Prefabs[i], spawnPos[i].position, spawnPos[i].rotation, parent.transform);
                Debug.Log("Step one done");
                mainList[i] = newSCP.GetComponent<Roam_NPC>();
            }
        }

        for (int i = 0; i < spawnSimps.Length; i++)
        {
            if (spawnSimps[i] == true)
            {
                GameObject newSCP = Instantiate(Simp_Prefabs[i], Vector3.zero, Quaternion.identity, parent.transform);
                simpList[i] = newSCP.GetComponent<SimpleNPC>();
            }
        }
    }

}
