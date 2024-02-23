using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using UnityEngine.Tilemaps;
using UnityEngine.Events;
using Volume = UnityEngine.Rendering.Volume;
using Unity.AI.Navigation;
using System.Linq;
using UnityEngine.AI;

public enum DeathEvent {none, pocketDimension, zombie008 };

[System.Serializable]
public class CameraPool
    {
        public Material Mats;
        public RenderTexture Renders;
        public bool isUsing;
    }

[System.Serializable]
public class savedDoor
{
    public int id;
    public bool isOpen;

    public savedDoor (int _id)
    {
        isOpen = false;
        id = _id;
    }

}

[System.Serializable]
public class savedObject
{
    public int id;
    public bool State;

    public savedObject(int _id)
    {
        State = false;
        id = _id;
    }

}

public class GameController : MonoBehaviour
{
    [Header("Map Settings")]
    [HideInInspector]
    public DeathEvent Death;
    public static GameController instance = null;
    public Worlds worldName;
    public bool isAlive = true;
    int doorCounter = 0;
    int persCounter = 0;
    public bool canSave = false, debugCamera, holdRoom = false;
    public bool CreateMap, ShowMap;
    public bool controlFog=true, customFog=false;
    public float defaultFog = 15f, fogSpeed = 3;
    public float toFog=15;
    public bool doGameplay, spawnPlayer, spawnHere, StopTimer = false, isStart = false, mapless;

    [Header("Volumes")]
    public UnityEngine.Rendering.Volume HorrorVol;
    public UnityEngine.Rendering.Volume MainVol;
    LiftGammaGain liftGamma;
    //DepthOfField depth;
    TweenBase HorrorTween;

    [HideInInspector]
    public int xPlayer, yPlayer;
    Camera HorrorFov;

    [Header("Start Config")]
    public GameObject origplayer;
    [HideInInspector]
    public GameObject player;
    [HideInInspector]
    public Player_Control playercache;
    public GameObject itemSpawner;
    public UnityEvent startGame=new();


    [HideInInspector]
    [System.NonSerialized]
    public GameObject itemParent, eventParent, doorParent, persParent;


    Transform currentTarget;

    public Transform WorldAnchor;

    int xStart, xEnd, yStart, yEnd;
    int Zone3limit, Zone2limit;
    int zoneAmbiance = -1;
    int zoneMusic = -1, currentMusic = -1;

    [HideInInspector]
    public bool CullerFlag, DebugFlag = false;
    bool CullerOn, playIntro = true;
    [HideInInspector]
    public int currZone = 0;
    [HideInInspector]
    public float roomsize = 15.3f;
    float Timer = 5, normalAmbiance;

    MapSize mapSize;
    int[,,] culllookup;
    int[,] Binary_Map;
    int[,] nav_Map;
    room[,] SCP_Map;
    RoomHolder[,] rooms;
    Dictionary<string, room_dat> roomLookup;


    ItemList[] itemData;
    [HideInInspector]
    public List<savedDoor> doorTable;
    public List<savedObject> persTable;



    public Transform playerSpawn;

    [Header("Audio Sources")]
    public AudioSource Ambiance;
    public AudioSource MixAmbiance;
    public AudioSource Horror;
    public AudioSource GlobalSFX;
    public AudioSource MenuSFX;

    AudioSource roomAmbiance_src;

    [Header("Audio Clips")]
    public AudioClip[] Z1;
    public AudioClip[] Z2;
    public AudioClip[] Z3;
    public AudioClip[] RoomMusic;
    public AudioClip[] roomAmbiance_clips;
    public AudioClip Mus1, Mus2, Mus3, savedSFX;

    bool RoomMusicChange = false;
    bool roomAmbiance_chg = false;
    Ambiances roomAmbiance_amb = Ambiances.drip;

    bool StartupDone = false;

    public List<int> globalInts = new();
    public List<bool> globalBools = new();
    public List<float> globalFloats = new();
    public List<string> globalStrings = new();

    /// <summary>
    /// SpecialItemsData
    /// </summary>
    /// 
    [Header("SNav Values")]
    public Tilemap mapFull;
    public TileBase tile;

    [Header("Graphics Settings")]
    public HDRenderPipelineAsset HDRPAsset;

    //public UnityEngine.ShadowResolution shadowQuality;

    [HideInInspector]
    public string deathmsg = "";
    [HideInInspector]
    public string currentRoom;

    //Systems
    [Header("Modules")]
    public ParticleController particleController;
    public NPC_Controller npcController;
    public AmbianceController ambianceController;
    public NewMapGen mapCreate;
    public GameObject roomAmbiance_obj;
    public CameraPool[] cameraPool;

    /// <summary>
    /// ////////////////////////STARTUP SEQUENCE
    /// </summary>

    void Awake()
    {

        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);


        itemParent = new GameObject("itemParent");

        eventParent = new GameObject("eventParent");


        doorParent = new GameObject
        {
            name = "doorParent"
        };

        persParent = new GameObject("persParent");


        if (GlobalValues.debug)
        {
            Localization.CheckLangs();
            Localization.SetLanguage(-1);
        }

    }

    private void Start()
    {
        
        roomAmbiance_obj = Instantiate(roomAmbiance_obj);
        roomAmbiance_src = roomAmbiance_obj.GetComponent<AudioSource>();
        MenuSFX.ignoreListenerPause = true;

        if(GlobalValues.isNewGame)
        {
            Debug.Log("Creando mundos");
            SaveSystem.instance.playData.worlds = new WorldData[SaveSystem.worldQ];
            SaveSystem.instance.playData.worldsCreateds = new bool[SaveSystem.worldQ];
            for (int i = 0; i < SaveSystem.worldQ; i++)
            {
                Debug.Log("i = " + i);
                SaveSystem.instance.playData.worldsCreateds[i] = false;
                SaveSystem.instance.playData.worlds[i] = new WorldData
                {
                    worldItems = new ItemList[100]
                };
            }
        }

        itemData = new ItemList[100];


        Time.timeScale = 0;
        doGameplay = false;

        if (!GlobalValues.debug && worldName!=Worlds.dontCare)
        {
            switch (GlobalValues.LoadType)
            {
                case LoadType.newgame:
                    {
                        spawnHere = false;
                        spawnHere = GlobalValues.playIntro;
                        mapCreate.mapgenseed = GlobalValues.mapseed;
                        NewGame();
                        break;
                    }

                case LoadType.loadgame:
                    {
                        SaveSystem.instance.LoadState();
                        Debug.Log("World name = " + worldName + " isCreated? " + SaveSystem.instance.playData.worldsCreateds[(int)worldName]);
                        if (SaveSystem.instance.playData.worldsCreateds[(int)worldName])
                            spawnHere = false;
                        LoadGame();
                        break;
                    }
                case LoadType.otherworld:
                    {
                        
                        SaveSystem.instance.playData = GlobalValues.worldState;
                        Debug.Log("World name = " + worldName + " isCreated? " + SaveSystem.instance.playData.worldsCreateds[(int)worldName]);
                        if (SaveSystem.instance.playData.worldsCreateds[(int)worldName])
                            spawnHere = false;
                        LoadGame();
                        break;
                    }
                case LoadType.mapless:
                    {

                        SaveSystem.instance.playData = GlobalValues.worldState;

                        ItemController.instance.EmptyItems();
                        ItemController.instance.LoadItems(SaveSystem.instance.playData.items, SaveSystem.instance.playData.equips);
                        globalInts = SaveSystem.instance.playData.globalInts;
                        globalFloats = SaveSystem.instance.playData.globalFloats;
                        globalStrings = SaveSystem.instance.playData.globalStrings;
                        globalBools = SaveSystem.instance.playData.globalBools;
                        spawnHere = true;
                        Debug.Log("Iniciando spawn mapless, con spawnHere valor " + spawnHere);


                        GL_SpawnPlayer(playerSpawn.position);
                        GL_Start();
                        GL_AfterPost();
                        break;
                    }
            }
        }

        if (worldName == Worlds.dontCare)
        {


            SaveSystem.instance.playData = GlobalValues.worldState;

            ItemController.instance.EmptyItems();
            ItemController.instance.LoadItems(SaveSystem.instance.playData.items, SaveSystem.instance.playData.equips);
            globalInts = SaveSystem.instance.playData.globalInts;
            globalFloats = SaveSystem.instance.playData.globalFloats;
            globalStrings = SaveSystem.instance.playData.globalStrings;
            globalBools = SaveSystem.instance.playData.globalBools;
            spawnHere = true;
            Debug.Log("Iniciando spawn mapless, con spawnHere valor " + spawnHere);


            GL_SpawnPlayer(playerSpawn.position);
            GL_Start();
            GL_AfterPost();

        }


        if (GlobalValues.debug && mapless)
        {
            GlobalValues.LoadType = LoadType.mapless;
            spawnHere = true;

            SaveSystem.instance.playData = GlobalValues.worldState;

            if (GlobalValues.worldState != null)
            {
                ItemController.instance.EmptyItems();
                ItemController.instance.LoadItems(SaveSystem.instance.playData.items, SaveSystem.instance.playData.equips);
            }

            GL_SpawnPlayer(playerSpawn.position);
            GL_AfterPost();
        }

        if (GlobalValues.debug && GlobalValues.LoadType == LoadType.otherworld)
        {
            spawnHere = false;
            SaveSystem.instance.playData = GlobalValues.worldState;
            LoadGame();
        }

    }

    void OnGUI()
    {
        if (!isStart && GlobalValues.debug && GlobalValues.LoadType != LoadType.otherworld && GlobalValues.LoadType != LoadType.mapless)
        {
            // Make a background box
            GUI.Box(new Rect(10, 10, 500, 120), "Menu Inicio");

            mapCreate.mapgenseed = GUI.TextField(new Rect(20, 40, 80, 20), mapCreate.mapgenseed);
            playIntro = GUI.Toggle(new Rect(120, 40, 80, 20), playIntro, "Iniciar Intro");

            if (GUI.Button(new Rect(220, 40, 80, 20), "Iniciar"))
            {
                NewGame();


            }
            if (GUI.Button(new Rect(220, 85, 80, 20), "Cargar"))
            {
                LoadGame();
            }
        }
        else if (DebugFlag)
        {
            GUI.Box(new Rect(10, 10, 300, 300), "Debug Data");
            GUI.Label(new Rect(20, 40, 300, 20), "Map X " + xPlayer + " Mapa Y " + yPlayer);
            GUI.Label(new Rect(20, 65, 300, 20), "This Zone " + currZone);
            GUI.Label(new Rect(20, 90, 300, 20), "Is Gameplay? " + doGameplay);
            GUI.Label(new Rect(20, 115, 300, 20), "Is Rooom hold? " + holdRoom);
            GUI.Label(new Rect(20, 130, 300, 20), "Is Pocket? " + (worldName == Worlds.pocket));
            GUI.Label(new Rect(20, 155, 300, 20), "is Alive? " + isAlive);
            GUI.Label(new Rect(20, 170, 300, 20), "Player X " + playercache.transform.position.x + " Y " + playercache.transform.position.y + " Z " + playercache.transform.position.z);
            GUI.Label(new Rect(20, 185, 300, 20), "Player Rotation " + playercache.transform.rotation.eulerAngles.y);
            GUI.Label(new Rect(20, 200, 300, 20), "Current room " + currentRoom);
            GUI.Label(new Rect(20, 215, 300, 20), "Asfixia " +playercache.AsfixiaRead);
            GUI.Label(new Rect(20, 230, 300, 20), "Health " + playercache.Health);
            GUI.Label(new Rect(20, 245, 300, 20), "Zombie " + playercache.zombieTimer);
            GUI.Label(new Rect(20, 260, 300, 20), "IsDebug " + GlobalValues.debug);
        }
    }


    void NewGame()
    {
        GL_PreStart();
        GL_NewStart();
        StartCoroutine(GL_PostStart());
    }

    void LoadGame()
    {
        GL_PreStart();
        GL_LoadStart();
        StartCoroutine(GL_PostStart());
    }

    public void LoadQuickSave()
    {
        /*if (GlobalValues.LoadType != LoadType.mapless)
        {*/
        SaveSystem.instance.LoadState();
        if(worldName!=SaveSystem.instance.playData.currentWorld)
            LoadingSystem.instance.LoadLevelHalf(GlobalValues.sceneTable[(int)SaveSystem.instance.playData.currentWorld], true, 1, 0, 0, 0);


        LoadingSystem.instance.FadeOut(0.1f, new Vector3Int(0, 0, 0));
        SCP_UI.instance.ToggleDeath();

        DestroyImmediate(itemParent);
        DestroyImmediate(eventParent);

        itemParent = new GameObject("itemParent");
        eventParent = new GameObject("eventParent");




        GL_Loading();

        Camera.main.gameObject.transform.parent = null;

        doorParent.BroadcastMessage("resetState");
        persParent.BroadcastMessage("resetState");

        DestroyImmediate(player);
        npcController.DeleteNPC();

        StartCoroutine(ReloadLevel());
        //}
        /*else
        {
            
            GlobalValues.isNew = false;
            GlobalValues.LoadType = LoadType.loadgame;
            GlobalValues.debug = false;
            LoadingSystem.instance.LoadLevelHalf(GlobalValues.sceneReturn, true, 1, 0, 0, 0);
        }*/

    }

    public int GetDoorID()
    {
        if (!SaveSystem.instance.playData.worldsCreateds[(int)worldName])
        {
            doorTable.Add(new savedDoor(doorTable.Count));
            return (doorTable.Count - 1);
        }
        else
        {
            doorCounter++;
            return (doorCounter - 1);
        }
    }

    public int GetDoorState(int id)
    {
        if (!SaveSystem.instance.playData.worldsCreateds[(int)worldName])
            return (-1);
        else
        {
            if (doorTable[id].isOpen == true)
                return (1);
            else
                return (0);
        }
    }

    public void SetDoorState(bool state, int id)
    {
        doorTable[id].isOpen = state;
    }

    public int GetObjectID()
    {
        if (!SaveSystem.instance.playData.worldsCreateds[(int)worldName])
        {
            persTable.Add(new savedObject(persTable.Count));
            return (persTable.Count - 1);
        }
        else
        {
            persCounter++;
            return (persCounter - 1);
        }
    }

    public int GetObjectState(int id)
    {
        if (!SaveSystem.instance.playData.worldsCreateds[(int)worldName])
            return (-1);
        else
        {
            if (persTable[id].State == true)
                return (1);
            else
                return (0);
        }
    }

    public void SetObjectState(bool state, int id)
    {
        persTable[id].State = state;
    }























    /// <summary>
    /// /////////////////////////////////////////////////////////////////////////////////////////////////GAMEPLAY
    /// </summary>
    public void Action_QuickSave()
    {
        SaveSystem.instance.playData = QuickSave();
        SaveSystem.instance.SaveState();
        GlobalSFX.PlayOneShot(savedSFX);
        GlobalValues.hasSaved = true;
        SubtitleEngine.instance.playSub("uiStrings","ui_in_saved");
    }

    void Update()
    {
        if (SCPInput.instance.playerInput.Gameplay.DebugF1.triggered)
        {
            DebugFlag = !DebugFlag;
        }


        if (isAlive)
        {
            if (SCPInput.instance.playerInput.Gameplay.Pause.triggered)
            {
                SCP_UI.instance.TogglePauseMenu();
            }

            if (SCPInput.instance.playerInput.Gameplay.Inventory.triggered)
            {
                SCP_UI.instance.ToggleInventory();
            }

            if (SCPInput.instance.playerInput.Gameplay.Save.triggered)
            {
                if (canSave)
                {
                    Action_QuickSave();
                }
                else
                {
                    SubtitleEngine.instance.playSub("uiStrings", "ui_in_nosave");
                }
            }
        }

        if (isStart)
        {
            /*if (spawnHere)
                StartIntro();*/


            if (doGameplay)
                Gameplay();
        }


    }



    public void DefaultAmbiance()
    {
        zoneAmbiance = 3;
        Debug.Log("Ambiance Default");
    }

    void AmbianceManager()
    {
        if (ambianceController.custom == false)
        {
            if (currZone == 2 && zoneAmbiance != 2)
            {
                ambianceController.NormalAmbiance(Z3);
                zoneAmbiance = 2;
            }
            if (currZone == 1 && zoneAmbiance != 1)
            {
                ambianceController.NormalAmbiance(Z2);
                zoneAmbiance = 1;
            }
            if (currZone == 0 && zoneAmbiance != 0)
            {
                ambianceController.NormalAmbiance(Z1);
                zoneAmbiance = 0;
            }

        }
        else
            zoneAmbiance = -1;
    }

    void MusicManager()
    {
        if (zoneMusic != -1)
        {
            if (currZone == 2 && zoneMusic != 2)
            {
                MusicPlayer.instance.ChangeMusic(Mus3);
                zoneMusic = 2;
            }
            if (currZone == 1 && zoneMusic != 1)
            {
                MusicPlayer.instance.ChangeMusic(Mus2);
                zoneMusic = 1;
            }
            if (currZone == 0 && zoneMusic != 0)
            {
                MusicPlayer.instance.ChangeMusic(Mus1);
                zoneMusic = 0;
            }

        }
    }




    public void ChangeMusic(AudioClip newMusic)
    {
        MusicPlayer.instance.ChangeMusic(newMusic);
        zoneMusic = -1;
    }

    public void DefMusic()
    {
        if (roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music != -1)
        {
            ChangeMusic(RoomMusic[roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music]);
            currentMusic = roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music;
            RoomMusicChange = true;
        }
        else
            zoneMusic = 3;
    }


    public void PlayHorror(AudioClip horrorsound, Transform origin, npc who)
    {
        //Debug.Log("Playing Horror");
        Horror.PlayOneShot(horrorsound);
        if (HorrorTween != null)
            HorrorTween.Cancel();
        HorrorTween = Tween.Value(0f, 1f, HorrorUpdate, 2f, 0f, Tween.EaseInStrong, Tween.LoopType.None, null, () => HorrorTween = Tween.Value(1f, 0f, HorrorUpdate, 6f, 0f, Tween.EaseOutStrong), true);
        if (origin != null)
        {
            currentTarget = origin;
        }

        if (who != npc.none)
        {
            npcController.npcLevel(who);
            Debug.Log("Playing horror for " + who);
        }
    }

    public void HorrorUpdate(float value)
    {
        Debug.Log("Horror Update " + value);
        if (worldName!=Worlds.pocket)
        {
            HorrorFov.fieldOfView = 75 + (7 * value);
        }

        HorrorVol.weight = value;
        //depth.focusDistance.Override(Vector3.Distance(player.transform.position, currentTarget.transform.position) - 1f);
    }


    void Gameplay()
    {
        if (!holdRoom)
        {
            int tempX = (Mathf.Clamp((Mathf.RoundToInt((player.transform.position.x / roomsize))), 0, mapSize.xSize - 1));
            int tempY = (Mathf.Clamp((Mathf.RoundToInt((player.transform.position.z / roomsize))), 0, mapSize.ySize - 1));
            if ((Binary_Map[tempX, tempY] != 0) && ((tempY == yPlayer && tempX == xPlayer + 1) || (tempY == yPlayer && tempX == xPlayer - 1) || (tempY == yPlayer + 1 && tempX == xPlayer) || (tempY == yPlayer - 1 && tempX == xPlayer)))
            {
                xPlayer = tempX;
                yPlayer = tempY;
                PlayerReveal(xPlayer, yPlayer);
                PlayerEvents();
                if (!customFog)
                    toFog = ((SCP_Map[xPlayer, yPlayer].customFog == -1) ? defaultFog : SCP_Map[xPlayer, yPlayer].customFog);
            }

            RenderSettings.fogEndDistance = Mathf.MoveTowards(RenderSettings.fogEndDistance, toFog, fogSpeed * Time.deltaTime);

            if (yPlayer < Zone3limit)
            {
                currZone = 2;
            }
            if (yPlayer > Zone3limit && yPlayer < Zone2limit)
            {
                currZone = 1;
            }
            if (yPlayer > Zone2limit)
            {
                currZone = 0;
            }
        }

        /*if (Input.GetKeyDown(KeyCode.F1))
        {
            if (npcPanel == false)
            {
                npcPanel = true;
                npcCam.SetActive(true);
            }
            else
            {
                npcPanel = false;
                npcCam.SetActive(false);
            }
        }*/



        if (npcController != null)
        {
            npcController.NPCManager();
        }

        
        MusicManager();

        if (ambianceController != null)
        {
            ambianceController.GenAmbiance();
            AmbianceManager();
        }



        if (CullerFlag == true && CullerOn == false)
        {
            StartCoroutine(RoomHiding());
        }
    }

    public void SetMapPos(int x, int y)
    {
        xPlayer = x;
        yPlayer = y;
        Debug.Log("Map Pos = " + x + " " + y);
        if (doGameplay)
        {
            if (SCP_Map[x, y].Event != -1)
            {
                rooms[x, y].GetComponent<EventHandler>().EventLoad(x, y, SCP_Map[x, y].eventDone);
            }
            PlayerReveal(x, y);
            PlayerEvents();
        }
    }


    /*void StartIntro()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0.0f && StopTimer == false)
        {
            startEv.SetActive(true);
            StopTimer = true;
        }
    }*/

    public int AddItem(Vector3 pos, gameItem item)
    {
        for (int i = 0; i < itemData.Length; i++)
        {
            if (itemData[i] == null || itemData[i].item == null)
            {
                itemData[i] = new ItemList
                {
                    X = pos.x,
                    Y = pos.y,
                    Z = pos.z
                };
                Debug.Log("Nuevoitem en: "+i);

                itemData[i].item = item;
                return (i);
            }
        }
        return (-1);
    }

    public void DeleteItem(int i)
    {
        //Debug.Log(i);
        itemData[i] = null;
    }



    public void setDone(int x, int y)
    {
        SCP_Map[x, y].eventDone = true;
    }

    public void setValue(int x, int y, int index, int value)
    {
        SCP_Map[x, y].values[index] = value;
    }

    public int getValue(int x, int y, int index)
    {
        return (SCP_Map[x, y].values[index]);
    }

    public GameObject getCutsceneObject(int x, int y, int index)
    {
        return rooms[x, y].cutsceneReferences[index];
    }

    void PlayerEvents()
    {
        Debug.Log("Executing room events @ " + SCP_Map[xPlayer, yPlayer].roomName);
        if (Binary_Map[xPlayer, yPlayer] != 0)
        {
            currentRoom = SCP_Map[xPlayer, yPlayer].roomName;

            if (SCP_Map[xPlayer, yPlayer].Event != -1)
            {
                Debug.Log("Executing room event");
                if (SCP_Map[xPlayer, yPlayer].eventDone != true)
                    rooms[xPlayer, yPlayer].GetComponent<EventHandler>().EventStart();
            }
            if (SCP_Map[xPlayer, yPlayer].items == 1)
            {
                Debug.Log("Spawning Items");
                rooms[xPlayer, yPlayer].GetComponent<Item_Spawner>().Spawn();
                SCP_Map[xPlayer, yPlayer].items = 2;
            }


            if (roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music != -1 && (RoomMusicChange == false || currentMusic != roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music))
            {
                Debug.Log("Changing music at room");
                ChangeMusic(RoomMusic[roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music]);
                currentMusic = roomLookup[SCP_Map[xPlayer, yPlayer].roomName].music;
                RoomMusicChange = true;
            }
            else
            {
                if (RoomMusicChange == true)
                    DefMusic();

                RoomMusicChange = false;
            }


            if (roomLookup[SCP_Map[xPlayer, yPlayer].roomName].hasAmbiance)
            {
                Debug.Log("Handling ambiance");
                AmbianceHandler handler = rooms[xPlayer, yPlayer].GetComponent<AmbianceHandler>();
                {
                    if (roomAmbiance_chg == false || roomAmbiance_amb != handler.Ambiance)
                    {
                        roomAmbiance_src.Stop();
                        roomAmbiance_chg = true;
                        roomAmbiance_src.clip = roomAmbiance_clips[(int)handler.Ambiance];
                    }
                    roomAmbiance_src.volume = handler.Volume;
                    roomAmbiance_src.spread = handler.spread;
                    roomAmbiance_src.minDistance = handler.closeDistance;

                    if (handler.hasOrigin)
                    {
                        roomAmbiance_src.spatialBlend = handler.spatial;
                        roomAmbiance_obj.transform.position = handler.origin.position;
                    }
                    else
                        roomAmbiance_src.spatialBlend = 0;

                    roomAmbiance_amb = handler.Ambiance;
                    roomAmbiance_src.Play();
                }
            }
            else
            {
                /*if (roomAmbiance_chg == true)
                    DefMusic();*/
                roomAmbiance_src.Stop();
                roomAmbiance_chg = false;
            }
            Debug.Log("Room events executed @ " + SCP_Map[xPlayer, yPlayer].roomName);
        }

    }










    /// <summary>
    /// ////////////////////////////////////////////////////////////NPC CODES
    /// </summary>

    public Vector3 GetPatrol(Vector3 MyPos, int Outer, int Inner)
    {
        int xPos = (Mathf.Clamp((Mathf.RoundToInt((MyPos.x / roomsize))), 0, mapSize.xSize - 1));
        int yPos = (Mathf.Clamp((Mathf.RoundToInt((MyPos.z / roomsize))), 0, mapSize.ySize - 1));
        Debug.Log("Recibi Posicion X= " + xPos + " Posicion Y= " + yPos);
        Debug.Log("Posicion X= " + xPlayer + " Posicion Y= " + yPlayer + " Hay cuarto? " + Binary_Map[xPlayer, yPlayer]);

        int xPatrol, yPatrol;

        do
        {
            xPatrol = Random.Range(Mathf.Clamp(xPos - Outer, 0, mapSize.xSize - 1), Mathf.Clamp(xPos + Outer, 0, mapSize.xSize - 1));
            yPatrol = Random.Range(Mathf.Clamp(yPos - Outer, 0, mapSize.ySize - 1), Mathf.Clamp(yPos + Outer, 0, mapSize.ySize - 1));
        }
        while (Binary_Map[xPatrol, yPatrol] == 0 || (((xPatrol < xPos + Inner) && (xPatrol > xPos - Inner)) || ((yPatrol < yPos + Inner) && (yPatrol > yPos - Inner))));

        Debug.Log("Otorgue Posicion X= " + xPatrol + " Posicion Y= " + yPatrol + " desde x " + xPos + " y " + yPos);

        return (new Vector3(xPatrol * roomsize, 0.0f, yPatrol * roomsize));
    }

    public bool PlayerNotHere(Vector3 MyPos)
    {
        int xPos = (Mathf.Clamp((Mathf.RoundToInt((MyPos.x / roomsize))), 0, mapSize.xSize - 1));
        int yPos = (Mathf.Clamp((Mathf.RoundToInt((MyPos.z / roomsize))), 0, mapSize.ySize - 1));

        return (xPos != xPlayer && yPos != yPlayer);
    }



    /// <summary>
    ///////////////////////////////////////////////////////// SNavCode
    /// </summary>
    /// 
    public void Map_Prepare()
    {
        nav_Map = new int[mapSize.xSize, mapSize.ySize];
        for (int x = 0; x < mapSize.xSize; x++)
        {
            //Loop through the height of the map
            for (int y = 0; y < mapSize.ySize; y++)
            {
                nav_Map[x, y] = 0;
            }
        }
    }

    public void Map_RenderFull()
    {
        //Clear the map (ensures we dont overlap)
        mapFull.ClearAllTiles();
        //Loop through the width of the map
        for (int x = 0; x < mapSize.xSize; x++)
        {
            //Loop through the height of the map
            for (int y = 0; y < mapSize.ySize; y++)
            {
                if (Binary_Map[x, y] == 1)
                {
                    mapFull.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }

    public void Map_RenderHalf()
    {
        //Clear the map (ensures we dont overlap)
        mapFull.ClearAllTiles();
        //Loop through the width of the map
        for (int x = 0; x < mapSize.xSize; x++)
        {
            //Loop through the height of the map
            for (int y = 0; y < mapSize.ySize; y++)
            {
                if (Binary_Map[x, y] == 1)
                {
                    mapFull.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }

        for (int x = 0; x < mapSize.xSize; x++)
        {
            //Loop through the height of the map
            for (int y = 0; y < mapSize.ySize; y++)
            {
                if (nav_Map[x, y] == 0)
                {
                    mapFull.SetColor(new Vector3Int(x, y, 0), Color.clear);
                }
                else
                {
                    mapFull.SetColor(new Vector3Int(x, y, 0), Color.white);
                }
            }
        }
    }



    /*public void LoadMap()
    {
        for (int x = 0; x < mapSize.xSize; x++)
        {
            //Loop through the height of the map
            for (int y = 0; y < mapSize.ySize; y++)
            {
                if (nav_Map[x, y] == 1)
                    mapFill.SetColor(new Vector3Int(x, y, 0), Color.white);
            }
        }
    }*/

    public void PlayerReveal(int x, int y)
    {
        mapFull.SetColor(new Vector3Int(x, y, 0), Color.white);
        nav_Map[x, y] = 1;
    }


    IEnumerator DeadMenu()
    {
        yield return new WaitForSeconds(8);
        SCP_UI.instance.ToggleDeath();
    }
    IEnumerator DoDeathEvent()
    {
        yield return new WaitForSeconds(8);
        switch (Death)
        {
            case DeathEvent.pocketDimension:
                {
                    SaveSystem.instance.playData = QuickSave();

                    GlobalValues.worldState = SaveSystem.instance.playData;

                    SeriVector temp = new(0, 0, 0);


                    LoadingSystem.instance.FadeOut(1.5f, new Vector3Int(0, 0, 0));
                    yield return new WaitForSeconds(3);
                    SaveSystem.instance.playData.worlds[(int)worldName].mainData[(int)npc.scp106].Pos = temp;
                    SaveSystem.instance.playData.worlds[(int)worldName].mainData[(int)npc.scp106].isActive = false;

                    GoPocket();
                    break;
                }
            case DeathEvent.zombie008:
                {
                    //GlobalValues.worldState = QuickSave();
                    //LoadingSystem.instance.FadeOut(1.5f, new Vector3Int(0, 0, 0));
                    yield return new WaitForSeconds(3);

                    GoZombie008();

                    break;
                }
        }
    }

    public void PlayerDeath()
    {
        doGameplay = false;
        StartCoroutine(DeadMenu());
        isAlive = false;
        CullerFlag = false;
    }
    public void FakeDeath()
    {
        StartCoroutine(DoDeathEvent());
        doGameplay = false;
        isAlive = false;
        CullerFlag = false;
    }








    /// <summary>
    /// ////////////////////////////////////////////////////////GAMEPLAY BACKEND
    /// </summary>



    public void GoMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoSafePlace()
    {
        SaveSystem.instance.playData = QuickSave();
        GlobalValues.LoadType = LoadType.mapless;
        GlobalValues.sceneReturn = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene("SafePlace");
    }
    public void GoPocket()
    {
        SaveSystem.instance.playData = QuickSave();

        GlobalValues.worldState = SaveSystem.instance.playData;
        GlobalValues.LoadType = LoadType.otherworld;
        GlobalValues.sceneReturn = SceneManager.GetActiveScene().buildIndex;
        //Debug.Log("Scene" + SceneManager.GetActiveScene().name);
        LoadingSystem.instance.LoadLevel(3);
    }
    public void GoZombie008()
    {
        GlobalValues.LoadType = LoadType.otherworld;
        //GlobalValues.sceneReturn = SceneManager.GetActiveScene().buildIndex;
        //Debug.Log("Scene" + SceneManager.GetActiveScene().name);
        LoadingSystem.instance.LoadLevel(4);
    }
    public void WorldReturn()
    {
        SaveSystem.instance.playData.items = ItemController.instance.GetItems();

        GlobalValues.LoadType = LoadType.otherworld;
        LoadingSystem.instance.LoadLevelHalf(1);
    }



    //////////////////////////////////GAME LOADING AND STARTUP///////////////////////////////////
    /// <summary>
    /// 
    /// </summary>
    /// 
    public void LoadUserValues()
    {
        SubtitleEngine.instance.LoadValues();
        SCP_UI.instance.LoadValues();
        HorrorFov.gameObject.GetComponent<Player_MouseLook>().LoadValues();
        //liftGamma.gamma.value = new Vector4(setValue(1, 1, 1, PlayerPrefs.GetFloat("Gamma", 0) * 2.5f);
        //Debug.Log(MainVol.profile.Has<LiftGammaGain>().gamma.value);
        //   if (MainVol.profile.Has<LiftGammaGain>())//.gamma.value = new Vector4(1, 1, 1, PlayerPrefs.GetFloat("Gamma", 0)*2.5F);
        //Debug.Log(PlayerPrefs.GetFloat("Gamma", 0));
        int qualitylevel = PlayerPrefs.GetInt("Quality");
        QualitySettings.SetQualityLevel(qualitylevel);
        PlayerPrefs.Save();
    }

    void LoadItems()
    {
        Debug.Log("Entrando al loop");
        for (int i = 0; i < itemData.Length; i++)
        {
            if (itemData[i] != null && itemData[i].item != null)
            {
                GameObject newObject;
                Debug.Log(itemData[i].item.itemFileName + " i: " + i);
                newObject = Instantiate(itemSpawner, new Vector3(itemData[i].X, itemData[i].Y + 0.2f, itemData[i].Z), Quaternion.identity);
                newObject.GetComponent<Object_Item>().item = itemData[i].item;
                newObject.GetComponent<Object_Item>().id = i;
                newObject.transform.parent = itemParent.transform;

                newObject.GetComponent<Object_Item>().Spawn();
            }
            else
            {
                itemData[i] = null;
            }
        }
    }






    public SaveData QuickSave()
    {
        SaveData playData = SaveSystem.instance.playData;
        Debug.Log("Salvando");
        playData.savedMap = SCP_Map;
        playData.saveName = GlobalValues.mapname;
        playData.saveSeed = GlobalValues.mapseed;
        playData.worlds[(int)worldName].doorState = doorTable;
        playData.worlds[(int)worldName].persState = persTable;
        
        playData.worlds[(int)worldName].playerPos = SeriVector.fromVector3(player.transform.position);
        playData.equips = ItemController.instance.GetEquips();
        playData.items = ItemController.instance.GetItems();
        playData.worlds[(int)worldName].angle = Camera.main.gameObject.transform.eulerAngles.y;
        if (ShowMap)
        {
            playData.navMap = nav_Map;
            playData.mapX = xPlayer;
            playData.mapY = yPlayer;
            playData.holdRoom = holdRoom;
            Debug.Log("Saved size = " + mapSize);
            playData.savedSize = mapSize;
        }
        playData.globalBools = globalBools;
        playData.globalFloats = globalFloats;
        playData.seedState = Random.state;
        playData.globalInts = globalInts;
        playData.globalStrings = globalStrings;
        playData.Health = playercache.Health;
        playData.bloodLoss = playercache.bloodloss;
        playData.zombieTime = (playercache.hasZombie == false ? -1 : playercache.zombieTimer);

        playData.worlds[(int)worldName].npcData = npcController.getData();
        playData.worlds[(int)worldName].mainData = npcController.getMain();
        playData.worlds[(int)worldName].simpData = npcController.getActiveSimps();
        playData.worlds[(int)worldName].worldItems = itemData;
        playData.worldsCreateds[(int)worldName] = true;


        return (playData);
    }



    void GL_PreStart()
    {
        if (ShowMap)
        {
            CullerFlag = false;
            CullerOn = false;

            Zone3limit = mapCreate.zone3_limit;
            Zone2limit = mapCreate.zone2_limit;
        }
    }


    IEnumerator GL_PostStart()
    {
        if (CreateMap)
        {
            if (ShowMap)
            {

                mapSize = mapCreate.mapSize;
                roomsize = mapCreate.roomsize;
                roomsize = mapCreate.roomsize;

                roomLookup = mapCreate.roomTable;
                rooms = mapCreate.mapobjects;

                yield return StartCoroutine(mapCreate.MostrarMundo());

                SCP_Map = mapCreate.DameMundo();
                Binary_Map = mapCreate.MapaBinario();

                culllookup = new int[mapSize.xSize, mapSize.ySize, 2];
                int i, j;
                for (i = 0; i < mapSize.xSize; i++)
                {
                    for (j = 0; j < mapSize.ySize; j++)
                    {
                        culllookup[i, j, 0] = 0;
                        culllookup[i, j, 1] = 0;
                    }
                }
                yield return StartCoroutine(HidAfterProbeRendering());
            }
        }

        GL_Spawning();

        if (!GlobalValues.debug)
        {
            LoadingSystem.instance.loadbar = 1f;
            LoadingSystem.instance.canClick = true;

            while (!LoadingSystem.instance.isLoadingDone)
            {
                yield return null;
            }
        }

        GL_Start();
        GL_AfterPost();
    }

    void GL_NewStart()
    {
        zoneAmbiance = -1;
        zoneMusic = -1;

        if (CreateMap)
        {
            mapCreate.CreaMundo();
        }
    }

    void GL_LoadStart()
    {
        
        GL_Loading();
        if (ShowMap)
        {
            zoneAmbiance = 3;
            zoneMusic = 3;

            mapCreate.mapfil = SaveSystem.instance.playData.savedMap;
            Debug.Log("Saved map size " + SaveSystem.instance.playData.savedMap);
            mapCreate.mapSize = SaveSystem.instance.playData.savedSize;

            GlobalValues.mapseed = SaveSystem.instance.playData.saveSeed;
            GlobalValues.mapname = SaveSystem.instance.playData.saveName;

            mapCreate.LoadingSave();
        }

    }




    void GL_Loading()
    {
        if (SaveSystem.instance.playData.worldsCreateds[(int)worldName])
        {
            itemData = SaveSystem.instance.playData.worlds[(int)worldName].worldItems;
            doorTable = SaveSystem.instance.playData.worlds[(int)worldName].doorState;
            persTable = SaveSystem.instance.playData.worlds[(int)worldName].persState;
        }

        nav_Map = SaveSystem.instance.playData.navMap;
        mapSize = SaveSystem.instance.playData.savedSize;
        mapCreate.mapfil = SaveSystem.instance.playData.savedMap;
        SCP_Map = SaveSystem.instance.playData.savedMap;

        ItemController.instance.EmptyItems();
        ItemController.instance.LoadItems(SaveSystem.instance.playData.items, SaveSystem.instance.playData.equips);
        holdRoom = SaveSystem.instance.playData.holdRoom;
        globalInts = SaveSystem.instance.playData.globalInts;
        globalFloats = SaveSystem.instance.playData.globalFloats;
        globalStrings = SaveSystem.instance.playData.globalStrings;
        globalBools = SaveSystem.instance.playData.globalBools;
    }


    void GL_AfterPost()
    {
        
        if (ShowMap)
        {
            if (GlobalValues.LoadType != LoadType.mapless)
            {
                if (SaveSystem.instance.playData.worldsCreateds[(int)worldName])
                {
                    doGameplay = true;
                    StopTimer = true;
                    LoadItems();
                    Camera.main.gameObject.transform.rotation = Quaternion.Euler(0, SaveSystem.instance.playData.worlds[(int)worldName].angle, 0);
                    SetMapPos(SaveSystem.instance.playData.mapX, SaveSystem.instance.playData.mapY);
                    ItemController.instance.SetEquips();
                    Random.state = SaveSystem.instance.playData.seedState;
                }
                Debug.Log("Showing map x" + xPlayer + " y " + yPlayer);
                var e = ShowRoom(xPlayer, yPlayer);
                while (e.MoveNext()) { }
            }
        }

        if (startGame != null)
            startGame.Invoke();

        isStart = true;
        HorrorFov = Camera.main;
        LoadUserValues();

        
        SCP_UI.instance.EnableMenu();
        PlayHorror(Z1[0], player.transform, npc.none);

        GlobalValues.isNewGame = false;
    }

    void GL_SpawnPlayer(Vector3 here)
    {
        Time.timeScale = 1;
        deathmsg = "";
        if (spawnPlayer)
        {
            if (worldName!=Worlds.dontCare && !SaveSystem.instance.playData.worldsCreateds[(int)worldName] && !spawnHere)
            {
                player = Instantiate(origplayer, WorldAnchor.transform.position + (Vector3.up * 0.5f), Quaternion.identity);
                Debug.Log("Spawning at anchor " + WorldAnchor.transform + " es nuevo " + !SaveSystem.instance.playData.worldsCreateds[(int)worldName] + " !spawnhere " + spawnHere);
            }
            else
            {
                if(worldName != Worlds.dontCare && SaveSystem.instance.playData.worldsCreateds[(int)worldName])
                    player = Instantiate(origplayer, here, Quaternion.Euler(0,SaveSystem.instance.playData.worlds[(int)worldName].angle,0));
                else
                    player = Instantiate(origplayer, here, Quaternion.identity);
                Debug.Log("Spawning here at " + here);
            }
            RuntimeNavmesh();
        }

        playercache = player.GetComponent<Player_Control>();
        if (!GlobalValues.isNewGame || GlobalValues.LoadType == LoadType.mapless && GlobalValues.debug == false)
        {
            playercache.Health = SaveSystem.instance.playData.Health;
            playercache.bloodloss = SaveSystem.instance.playData.bloodLoss;
            playercache.hasZombie = SaveSystem.instance.playData.zombieTime > 0;
            playercache.zombieTimer = SaveSystem.instance.playData.zombieTime;
        }
        if (GlobalValues.debug == true)
        {
            playercache.isGameplay = true;
        }
    }

    void GL_Start()
    {
        if (worldName != Worlds.dontCare && SaveSystem.instance.playData.worldsCreateds[(int)worldName] && GlobalValues.LoadType != LoadType.mapless)
        {
            Debug.Log("Spawning inplaces");
            npcController.ResetNPC(SaveSystem.instance.playData.worlds[(int)worldName].npcData, SaveSystem.instance.playData.worlds[(int)worldName].mainData, SaveSystem.instance.playData.worlds[(int)worldName].simpData);
        }
        Camera.main.enabled = true;
        playercache.isGameplay = true;
    }
    
    void RuntimeNavmesh()
    {
        //update navmesh data for runtime navmesh
    }
    
    void GL_Spawning()
    {
        Vector3 here = playerSpawn.position;
        bool origSpawn = spawnHere;
        if (worldName != Worlds.dontCare && SaveSystem.instance.playData.worldsCreateds[(int)worldName] && GlobalValues.LoadType != LoadType.mapless)
        {
            spawnHere = true;
            here = SaveSystem.instance.playData.worlds[(int)worldName].playerPos.toVector3();
        }


        GL_SpawnPlayer(here);

        npcController.GL_Spawn();


        spawnHere = origSpawn;
        Debug.Log("Voy a mostrar el mapa? " + ShowMap);
        if (ShowMap)
        {
            Map_Prepare();

            if (SaveSystem.instance.playData.worldsCreateds[(int)worldName] && ShowMap)
            {
                nav_Map = SaveSystem.instance.playData.navMap;
                SetMapPos(SaveSystem.instance.playData.mapX, SaveSystem.instance.playData.mapY);
            }
            else
                SetMapPos(0, 10);
        }
    }

















    //////////////////////////////////////////////////////////////////////CULLING AND STARTUP////////////////////////////////////////////
    /// <summary>
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    /// <returns></returns>
    /// 

    void HidRoom(int i, int j)
    {
        culllookup[i, j, 1] = 0;
        if (SCP_Map[i, j].Event != -1)
        {
            rooms[i, j].GetComponent<EventHandler>().EventUnLoad();
        }

        RoomHolder hold = rooms[i, j];
        hold.Lights.SetActive(false);
        if (hold.Probes != null)
            hold.Probes.SetActive(false);

        Renderer[] rs = hold.Room.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rs)
            r.enabled = false;
    }

    IEnumerator ShowRoom(int i, int j)
    {
        Debug.Log("Showing room " + rooms[i, j].name + " en x" + i + " y " + j);
        if (SCP_Map[i, j].Event != -1)
        {
            Debug.Log("Loading events " + rooms[i, j].name);
            rooms[i, j].GetComponent<EventHandler>().EventLoad(i, j, SCP_Map[i, j].eventDone);
        }
        culllookup[i, j, 1] = 1;
        culllookup[i, j, 0] = 1;

        yield return null;

        RoomHolder hold = rooms[i, j];
        Debug.Log("Activating lights " + rooms[i, j].name);
        hold.Lights.SetActive(true);

        Renderer[] rs = hold.Room.GetComponentsInChildren<Renderer>();
        Debug.Log("Activating renderers " + rooms[i, j].name);
        foreach (Renderer r in rs)
            r.enabled = true;

        if (hold.Probes != null)
        {
            hold.Probes.SetActive(true);
            Debug.Log("Rendering probes " + rooms[i, j].name);
            yield return StartCoroutine(RenderProbe(hold.Probes.GetComponentsInChildren<ReflectionProbe>()));
        }
    }

    IEnumerator RenderProbe(ReflectionProbe[] probes)
    {
        foreach (ReflectionProbe probe in probes)
        {
            bool lazy = true;
            if (probe != null)
            {
                int timeout = 0;
                probe.timeSlicingMode = lazy ? UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces : UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
                int renderID;
                renderID = probe.RenderProbe();
                //int waitFrames = lazy ? 6 : 1;
                /*for (int i = 0; i < waitFrames;i++)
                {
                    yield return null;
                }*/
                while (!probe.IsFinishedRendering(renderID) && timeout < 10)
                {
                    timeout++;
                    yield return null;
                }
            }

        }
    }



    IEnumerator HidAfterProbeRendering()
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(GlobalValues.renderTime);
        int i, j;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if ((Binary_Map[i, j] == 1))      //Imprime el mapa
                {
                    //Debug.Log("Hiding Room at x" + i + " y " + j);
                    HidRoom(i, j);
                    yield return null;
                }
            }
        }

    }

    IEnumerator ReloadLevel()
    {
        Time.timeScale = 1;
        GL_Spawning();
        yield return new WaitForSeconds(5);
        int i, j;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if ((Binary_Map[i, j] == 1))      //Imprime el mapa
                {
                    HidRoom(i, j);
                    culllookup[i, j, 0] = 0;
                    culllookup[i, j, 1] = 0;
                }
            }
        }
        
        GL_Start();
        LoadingSystem.instance.FadeIn(0.5f, new Vector3Int(0, 0, 0));

        //canSave = true;
        //doGameplay = true;
        isAlive = true;
        //LoadItems();

        CullerFlag = true;

        GL_AfterPost();
    }


    IEnumerator RoomHiding()
    {
        CullerOn = true;
        int i, j;
        xStart = Mathf.Clamp(xPlayer - 2, 0, mapSize.xSize);
        xEnd = Mathf.Clamp(xPlayer + 2, 0, mapSize.xSize);
        yStart = Mathf.Clamp(yPlayer - 2, 0, mapSize.ySize);
        yEnd = Mathf.Clamp(yPlayer + 2, 0, mapSize.ySize);

        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                culllookup[i, j, 0] = 0;
            }
        }

        for (i = xStart; i < xEnd; i++)
        {
            for (j = yStart; j < yEnd; j++)
            {
                if ((Binary_Map[i, j] == 1))      //Imprime el mapa
                {
                    if (culllookup[i, j, 1] == 1)
                        culllookup[i, j, 0] = 1;
                    else
                    {
                        //Debug.Log("Showing Room at x" + i + " y " + j);
                        yield return ShowRoom(i, j);
                        
                    }
                }
            }
        }

        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if (culllookup[i, j, 0] == 1)
                    culllookup[i, j, 1] = 1;
                if (culllookup[i, j, 0] == 0)
                {
                    if (culllookup[i, j, 1] == 1)
                    {
                        HidRoom(i, j);
                        yield return null;
                    }
                }
            }
        }

        //Debug.Log("Culling Routine ended, waiting for next start");
        CullerOn = false;
        yield return null;
    }

























    ////////////////////////////CONSOLE COMMANDS/////////////////////////////////////
    ///
    public void TeleportCoord(int x, int y)
    {
        SetMapPos(x, y);
        player.GetComponent<Player_Control>().playerWarp(new Vector3(roomsize * x, 1, roomsize * y), 0);
    }

    public bool TeleportRoom(string room)
    {
        for (int x = 0; x < mapSize.xSize; x++)
        {
            for (int y = 0; y < mapSize.ySize; y++)
            {
                if (Binary_Map[x, y] != 0)
                {
                    if (SCP_Map[x, y].roomName.Equals(room))
                    {
                        TeleportCoord(x, y);
                        return(true);
                    }
                }
            }
        }
        return (false);
    }

    public void CL_spawn106()
    {
        Vector3 here = new(xPlayer * roomsize, 0, yPlayer * roomsize);
        npcController.mainList[(int)npc.scp106].Spawn(true, here);
    }
    public void CL_spawn049()
    {
        Vector3 here = new(xPlayer * roomsize, 0, yPlayer * roomsize);
        npcController.mainList[(int)npc.scp049].Spawn(true, here);
    }
    public void CL_spawn096()
    {
        Vector3 here = new(xPlayer * roomsize, 0, yPlayer * roomsize);
        npcController.mainList[(int)npc.scp096].Spawn(true, here);
    }
    public void CL_spawn173()
    {
        Vector3 here = new(xPlayer * roomsize, 0, yPlayer * roomsize);
        npcController.mainList[(int)npc.scp173].Event_Spawn(true, here);
    }
    public void CL_spawn513()
    {
        npcController.simpList[(int)SimpNpcList.bell].isActive = true;
    }
}
