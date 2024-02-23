using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class LoadFileButton : MonoBehaviour
{
    public string SaveName;
    public string SavePath;
    public string Date;
    public Text textName;
    public Text textDate;
    public Text textSeed;
    public Text textVer;
    public GameObject loadButton;
    public saveMeta meta;
    // Start is called before the first frame update
    void Start()
    {
        textName.text = SaveName;
        textDate.text = Date;

        if(File.Exists((SavePath.Replace(GlobalValues.fileExtension, GlobalValues.metaExtension))))
        {
            using (StreamReader streamReader = File.OpenText((SavePath.Replace(GlobalValues.fileExtension, GlobalValues.metaExtension))))
            {
                string jsonString = streamReader.ReadToEnd();
                meta = JsonUtility.FromJson<saveMeta>(jsonString);
            }

            textVer.text = "Ver. " + meta.mapver;
            textSeed.text = meta.seed;
            if (meta.mapver != GlobalValues.saveFileVer)
                loadButton.SetActive(false);
        }
        else
        {
            textVer.text = "Ver. 0.1.0";
            textSeed.text = "N/A";
            loadButton.SetActive(false);
        }
    }

    // Update is called once per frame
    public void DeleteThis()
    {
        File.Delete(SavePath);
        File.Delete((SavePath.Replace(GlobalValues.fileExtension, GlobalValues.metaExtension)));
        DestroyImmediate(this.gameObject);
    }

    public void StartThis()
    {
        GlobalValues.pathfile = meta.savepath;
        GlobalValues.mapname = SaveName;
        GlobalValues.isNewGame = false;
        GlobalValues.playIntro = false;
        GlobalValues.hasSaved = true;
        GlobalValues.LoadType = LoadType.loadgame;
        SCP_StartMenu.instance.Load_CB();
    }
}
