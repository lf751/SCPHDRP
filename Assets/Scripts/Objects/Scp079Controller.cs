using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scp079Controller : MonoBehaviour
{
    public static Scp079Controller ins;
    [System.NonSerialized]
    public bool can895 = true;
    public float chance895, min895Time, max895Time, disToCam, minFlickerTime, maxFlickerTime, currFlickTime, minLightTime, maxLightTime, lightTime, doorChance, doorMinTime, doorMaxTime, currDoorTime, doorDis;
    float resInten;
    bool hackingLight;
    Light flickLight;

    public AudioClip[] HackingSounds;
    public AudioClip[] FlickSounds;
    public AudioClip DoorJumpscare;
    public Texture2D scp079Picture;

    // Start is called before the first frame update
    private void Awake()
    {
        ins = this;
        GameController.ins.startGame.AddListener(OnGameStart);
        Debug.Log("Loaded 079");

    }

    private void OnDestroy()
    {
        GameController.ins.startGame.RemoveListener(OnGameStart);
    }

    private void OnGameStart()
    {
        Debug.Log(" 079 game start code");
        hackingLight = false;
        currFlickTime = Random.Range(minFlickerTime, maxFlickerTime);
        currDoorTime = Random.Range(doorMinTime, doorMaxTime);
        //Debug.Log("Is new world?" + GlobalValues.isNewGame);
        if (GlobalValues.isNewGame)
        {
            for (int x = 0; x < GameController.ins.mapCreate.mapSize.xSize; x++)
            {
                for (int y = 0; y < GameController.ins.mapCreate.mapSize.ySize; y++)
                {
                    if ((GameController.ins.rooms[x, y] != null))      //Imprime el mapa
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            //Debug.Log("Door @ " + x + ", " + y + ", " + i + ", is= " + GameController.ins.rooms[x, y].doors[i]);
                            if (GameController.ins.rooms[x, y].doors[i] != null)
                            {
                                float currDoorChance = Random.value;
                                if(currDoorChance < doorChance)
                                {
                                    //Debug.Log("Door Opened @ " + x + ", " + y);
                                    GameController.ins.rooms[x, y].doors[i].switchOpen = true;
                                    GameController.ins.rooms[x, y].doors[i].IsOpen = false;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.ins.isAlive && GameController.ins.doGameplay)
        {
            if (GameController.ins.globalBools[6])
            {
                currFlickTime -= Time.deltaTime;
                if (currFlickTime < 0&&!hackingLight)
                {
                    if (Time.frameCount % 15 == 0)
                    {
                        Light[] lights = GameController.ins.rooms[GameController.ins.xPlayer, GameController.ins.yPlayer].Lights.GetComponentsInChildren<Light>();
                        HashSet<int> checkedLights = new HashSet<int>();
                        
                        for(int i = 0; i < lights.Length; i++)
                        {
                            int randIdx = Random.Range(0, lights.Length);
                            while (checkedLights.Contains(randIdx))
                            {
                                randIdx = Random.Range(0, lights.Length);
                            }

                            checkedLights.Add(randIdx);

                            if(Lib.IsInView(GameController.ins.currPly.PlayerCam, lights[randIdx].transform)&&Lib.SqrDistance(GameController.ins.currPly.PlayerCam.transform.position, lights[randIdx].transform.position)<(10f*10f))
                            {
                                lightTime = Random.Range(minLightTime, maxLightTime);
                                flickLight = lights[randIdx];
                                resInten = flickLight.intensity;
                                hackingLight = true;
                                GameController.ins.GlobalSFX.PlayOneShot(FlickSounds[Random.Range(0, FlickSounds.Length)]);
                                break;
                            }
                        }
                    }
                }

                currDoorTime -= Time.deltaTime;
                if (currDoorTime < 0)
                {
                    //Debug.Log("Door jump time");
                    if (Time.frameCount % 15 == 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            if (GameController.ins.rooms[GameController.ins.xPlayer, GameController.ins.yPlayer].doors[i] != null)
                            {
                                Object_Door door = GameController.ins.rooms[GameController.ins.xPlayer, GameController.ins.yPlayer].doors[i];
                                //Debug.Log("Chceking Door x=" + GameController.ins.xPlayer + " y=" + GameController.ins.yPlayer + " i=" + i+ " dis: " + door.transform.position.SqrDistance(GameController.ins.currPly.transform.position));
                                if (door.transform.position.SqrDistance(GameController.ins.currPly.transform.position) < (doorDis * doorDis))
                                {
                                    if (door.switchOpen)
                                    {
                                        currDoorTime = Random.Range(doorMinTime, doorMaxTime);
                                        door.DoorSwitch(false);
                                        GameController.ins.Horror.PlayOneShot(DoorJumpscare);
                                    }
                                    else
                                    {
                                        currDoorTime = 1.5f;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (hackingLight)
                {
                    flickLight.intensity = resInten;
                    hackingLight = false;
                }
            }

            if (hackingLight)
            {
                lightTime -= Time.deltaTime;
                float flick = Lib.EvalWave(WaveFunctions.Noise, 0, 0.5f, 0.5f);
                //Debug.Log("Flick: " + flick);
                flickLight.intensity = resInten + flick;
                if (lightTime < 0)
                {
                    flickLight.intensity = resInten;
                    currFlickTime = Random.Range(minFlickerTime, maxFlickerTime);
                    hackingLight = false;
                }
            }
        }
    }
}
