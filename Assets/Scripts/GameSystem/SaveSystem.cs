using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;




[System.Serializable]
public enum Worlds { facility, pocket, russia, dontCare = -1 };


/// <summary>
/// Items del juego
/// </summary>
/// 
[System.Serializable]
public class ItemList
{
    public float X, Y, Z;
    [SerializeReference]
    public GameItem item;
    public ItemList()
    {
        item = null;
    }
}

[System.Serializable]
public class SeriVector
{
    public float x, y, z;

    public SeriVector(float _x, float _y, float _z)
    {
        x = _x;
        y = _y;
        z = _z;

    }

    static public SeriVector fromVector3(Vector3 og)
    {
        return new SeriVector(og.x, og.y, og.z);
    }

    public Vector3 toVector3()
    {
        return (new Vector3(x, y, z));
    }
}

public class saveMeta
{
    public string seed;
    public string mapver;
    public string gamever;
    public string savepath;

    public saveMeta(string _seed, string _path)
    {
        seed = _seed;
        mapver = GlobalValues.saveFileVer;
        gamever = Application.version;
        savepath = _path;
    }
}

[System.Serializable]
public class WorldData
{
    [SerializeReference]
    public ItemList[] worldItems;
    [SerializeReference]
    public NPC_Data[] npcData;
    [SerializeReference]
    public NPC_Data[] mainData;
    public bool[] simpData;

    public List<savedDoor> doorState;
    public List<int> persState;

    public SeriVector playerPos;
    public float angle;
}

[System.Serializable]
public class SaveData
{

    public string saveName;
    public string saveSeed;
    public room[,] savedMap;
    public int[,] navMap;
    public MapSize savedSize;
    public float Health, bloodLoss, zombieTime, leftRun, leftBlink, coffinTime;

    public int mapX, mapY;
    [SerializeReference]
    public List<GameItem[]> items;
    public List<bool[]> equips;

    public Worlds currentWorld;
    //[SerializeReference]
    public WorldData[] worlds;
    public bool[] worldsCreateds;

    public bool holdRoom;
    public Random.State seedState;

    public List<int> globalInts;
    public List<bool> globalBools;
    public List<float> globalFloats;
    public List<string> globalStrings;
}


public class SaveSystem : MonoBehaviour
{
    public const int worldQ = 3;

    public SaveData playData = new SaveData();
    public static SaveSystem instance = null;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
    }



    public void SaveState()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, GlobalValues.folderName);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        playData.saveName = GlobalValues.mapname;

        string dataPath = Path.Combine(folderPath, playData.saveName + GlobalValues.fileExtension);
        GlobalValues.pathfile = dataPath;
        WriteSaveFile(playData, dataPath);

        string metaPath = Path.Combine(folderPath, playData.saveName + ".meta");

        string jsonString = JsonUtility.ToJson(new saveMeta(GlobalValues.mapseed, dataPath));

        using (StreamWriter streamWriter = File.CreateText(metaPath))
        {
            streamWriter.Write(jsonString);
        }
    }

    public void LoadState()
    {
        /*string[] filePaths = GetFilePaths();

        if (filePaths.Length > 0)
        {*/
        playData = LoadSaveFile(GlobalValues.pathfile);
        //Debug.Log("Lo cargue! " + GlobalValues.pathfile);
        /*}
        else
            Debug.Log("No encontre nada");*/
    }




    void WriteSaveFile(SaveData data, string path)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate))
        {
            binaryFormatter.Serialize(fileStream, data);
            Debug.Log("Saved in " + path);
        }
    }

    static SaveData LoadSaveFile(string path)
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();

        using (FileStream fileStream = File.Open(path, FileMode.Open))
        {
            Debug.Log("Loading from " + path);
            return (SaveData)binaryFormatter.Deserialize(fileStream);

        }
    }

    /*string[] GetFilePaths()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, folderName);
        folderPath = folderPath.Replace("/", @"\");
        Debug.Log(folderPath);

        return Directory.GetFiles(folderPath, "*"+fileExtension);
    }*/
}
