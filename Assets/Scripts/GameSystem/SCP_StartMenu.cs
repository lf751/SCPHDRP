using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class SCP_StartMenu : MonoBehaviour
{
    Dictionary<int, Language> langs;
    public Canvas mainMenu, playMenu, newMenu, currMenu, loadMenu, optionMenu, creditsMenu;
    public GameObject saveList;
    public GameObject saveSlot;
    public AudioSource player;
    public AudioClip click;
    public AudioClip Menu;
    public InputField seedString, namestring;
    public Button mapnew;
    public string[] seeds;

    public static SCP_StartMenu instance = null;
    //public bool forceEnglish;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        Localization.CheckLangs();

        langs = Localization.GetLangs();

        Debug.Log("Language was " + PlayerPrefs.GetInt("Lang", 0));

        switch (PlayerPrefs.GetInt("Lang", 0))
        {
            case 0:
                {
                    Localization.SetLanguage(-1);
                    break;
                }
            default:
                {
                    Localization.SetLanguage(langs[PlayerPrefs.GetInt("Lang", 0)].unitynumber);
                    break;
                }
        }

        currMenu = mainMenu;
    }



    private void Start()
    {
        MusicPlayer.instance.StartMusic(Menu);
        GlobalValues.playIntro = true;
        Time.timeScale = 1;
        AudioListener.pause = false;
    }


    public void OpenPlay()
    {
        currMenu.enabled = false;
        playMenu.enabled = true;
        currMenu = playMenu;

        player.PlayOneShot(click);
    }
    public void OpenNew()
    {
        currMenu.enabled = false;
        newMenu.enabled = true;
        currMenu = newMenu;
        seedString.text = seeds[Random.Range(0, seeds.Length)];
        GlobalValues.mapseed = seedString.text;
        player.PlayOneShot(click);

        if (string.IsNullOrWhiteSpace(namestring.text) || string.IsNullOrWhiteSpace(seedString.text))
            mapnew.interactable = false;
        else
            mapnew.interactable = true;
    }
    public void OpenLoad()
    {
        var Files = GetFilePaths();
        foreach (string file in Files)
        {
            GameObject newSlot = Instantiate(saveSlot, saveList.transform);
            Debug.Log(Path.GetFileNameWithoutExtension(file));
            newSlot.GetComponent<LoadFileButton>().SaveName = Path.GetFileNameWithoutExtension(file);
            newSlot.GetComponent<LoadFileButton>().SavePath = file;
            newSlot.GetComponent<LoadFileButton>().Date = new FileInfo(file).CreationTime.ToString();
        }

        currMenu.enabled = false;
        loadMenu.enabled = true;
        currMenu = loadMenu;
        player.PlayOneShot(click);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void CloseLoad()
    {
        foreach (Transform child in saveList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }



        currMenu.enabled = false;
        playMenu.enabled = true;
        currMenu = playMenu;
        player.PlayOneShot(click);
    }

    public void OpenMain()
    {
        currMenu.enabled = false;
        mainMenu.enabled = true;
        currMenu = mainMenu;
        player.PlayOneShot(click);
    }

    public void OpenOption()
    {
        currMenu.enabled = false;
        optionMenu.enabled = true;
        currMenu = optionMenu;
        player.PlayOneShot(click);
    }

    public void OpenCredits()
    {
        currMenu.enabled = false;
        creditsMenu.enabled = true;
        currMenu = creditsMenu;
        player.PlayOneShot(click);
    }

    public void StartGame()
    {
        player.PlayOneShot(click);
        if (string.IsNullOrWhiteSpace(seedString.text))
            GlobalValues.mapseed = GlobalValues.getRandomString(5, 9);

        GlobalValues.isNewGame = true;
        GlobalValues.hasSaved = false;
        GlobalValues.LoadType = LoadType.newgame;
        Load_CB();
    }

    public void SetSeed(string seed)
    {
        Debug.Log(seed);
        if (string.IsNullOrWhiteSpace(namestring.text))
            mapnew.interactable = false;
        else
            mapnew.interactable = true;
        GlobalValues.mapseed = seed;
    }
    public void SetName(string name)
    {
        Debug.Log(name);
        if (string.IsNullOrWhiteSpace(namestring.text))
            mapnew.interactable = false;
        else
            mapnew.interactable = true;

        GlobalValues.mapname = name;
    }

    IOrderedEnumerable<string> GetFilePaths()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, GlobalValues.folderName);
        folderPath = folderPath.Replace("/", @"\");
        Debug.Log(folderPath);

        return Directory.GetFiles(folderPath, "*" + GlobalValues.fileExtension).OrderByDescending(d => new FileInfo(d).CreationTime); ;
    }

    public void Load_CB()
    {
        GlobalValues.playername = "[REDACTED]";
        GlobalValues.design = "D-9341";
        GlobalValues.debug = false;
        if (GlobalValues.isNewGame)
            LoadingSystem.instance.LoadLevelHalf(1, true, 1, 255, 255, 255);
        else
            LoadingSystem.instance.LoadLevelHalf(1, true, 1, 0, 0, 0);
    }

    public void PlayIntro(bool value)
    {
        GlobalValues.playIntro = value;

    }
}