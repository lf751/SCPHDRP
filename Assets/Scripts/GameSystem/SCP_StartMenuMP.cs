using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using Unity.Netcode;
using System;
using UnityEditor;

namespace LobbyRelaySample.ngo
{
    public class SCP_StartMenuMP : NetworkBehaviour
    {
        public Canvas playMenu, newMenu, currMenu, loadMenu;
        public GameObject saveList;
        public GameObject saveSlot;
        public AudioSource player;
        public AudioClip click;
        public AudioClip Menu;
        public InputField seedString, namestring;
        public Button mapnew;
        [SerializeField]
        private string[] seeds;

        public static SCP_StartMenuMP Instance
        {
            get
            {
                if (s_Instance!) return s_Instance;
                return s_Instance = FindFirstObjectByType<SCP_StartMenuMP>();
            }
        }

        static SCP_StartMenuMP s_Instance;

        public static SCP_StartMenuMP instance = null;
        //public bool forceEnglish;
        // Start is called before the first frame update
        void Awake()
        {
            currMenu = playMenu;
        }



        private void Start()
        {
            MusicPlayer.instance.StartMusic(Menu);
            GlobalValues.playIntro = false;
            Time.timeScale = 1;
            AudioListener.pause = false;
        }

        public void OpenNew()
        {
            currMenu.enabled = false;
            newMenu.enabled = true;
            currMenu = newMenu;
            seedString.text = seeds[UnityEngine.Random.Range(0, seeds.Length)];
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

        public void MainMenu()
        {
            SceneManager.LoadScene(0);
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
}