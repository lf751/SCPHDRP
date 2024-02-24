using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CbMode : MonoBehaviour
{
    bool ambiancechanged = false;
    string codeGlyphs = "0123456789";
    public GameObject fog;
    bool fogIsActive = false;
    // Start is called before the first frame update
    void Start()
    {
        if (GlobalValues.isNewGame)
        {
            GameController.ins.globalBools.Add(false); //HEAVYZONE Open 0
            GameController.ins.globalBools.Add(false); //OFFICEZONE Open 1
            GameController.ins.globalBools.Add(false); //DEMO LOCK 2
            GameController.ins.globalBools.Add(true); //LIGHTS ENABLED 3
            GameController.ins.globalBools.Add(false); //106 ENABLED 4
            GameController.ins.globalBools.Add(false); //PocketOnce 5
            GameController.ins.globalBools.Add(true); //Door Control On 6
            GameController.ins.globalBools.Add(true); //895 footage on 7

            GameController.ins.globalStrings.Add(getRandomString(4));//Maintenance code
            GameController.ins.globalStrings.Add(getRandomString(4));//Maynard code

            GameController.ins.globalFloats.Add(Random.Range(840, 1050)); //LARRY TIMER
            GameController.ins.globalFloats.Add(180); //AMBIANCETIMER
        }
        GameController.ins.startGame.AddListener(GameStart);
        fogIsActive = false;
        //GameController.ins.UpdateLights();
    }

    private void OnDestroy()
    {
        GameController.ins.startGame.RemoveListener(GameStart);
    }

    void GameStart()
    {


        Debug.Log("LightsOn is " + GameController.ins.lightsOn);
        GameController.ins.lightsOn = GameController.ins.globalBools[3];
    }

    public string getRandomString(int length)
    {
        string myString = "";
        for (int i = 0; i < length; i++)
        {
            myString += codeGlyphs[Random.Range(0, codeGlyphs.Length)];
        }
        return myString;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.ins.doGameplay)
        {
            if (!fogIsActive)
            {
                fog.SetActive(true);
                fogIsActive = true;

            }
            else
            {
                fog.transform.position = GameController.ins.player.transform.position;
            }
            if (GameController.ins.currZone == 0)
            {
                GameController.ins.globalBools[0] = true;
            }

            float mult106 = GameController.ins.globalBools[6] ? 1 : 4;
            GameController.ins.globalFloats[0] -= Time.deltaTime * mult106;
            GameController.ins.globalFloats[1] -= Time.deltaTime;
            if (GameController.ins.globalFloats[0] < 0)
            {
                GameController.ins.globalFloats[0] = Random.Range(840, 1050); //LARRY TIMER
                Vector3 newpos = GameController.ins.player.transform.position;
                newpos.y -= 1f;
                GameController.ins.npcController.mainList[(int)npc.scp106].Spawn(true, newpos);
            }

            if (GameController.ins.globalFloats[1] < 0 && !ambiancechanged)
            {
                GameController.ins.DefaultAmbiance();
                ambiancechanged = true;
            }

            GameController.ins.lightsOn = GameController.ins.globalBools[3];
        }
    }
}
