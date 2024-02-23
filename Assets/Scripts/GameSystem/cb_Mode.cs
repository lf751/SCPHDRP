using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cb_Mode : MonoBehaviour
{
    bool ambiancechanged = false;
    string codeGlyphs = "0123456789";
    // Start is called before the first frame update
    void Start()
    {
        if (GlobalValues.isNewGame)
        {
            GameController.instance.globalBools.Add(false); //HEAVYZONE LOCK
            GameController.instance.globalBools.Add(false); //LIGHTZONE LOCK
            GameController.instance.globalBools.Add(true); //DEMO LOCK
            GameController.instance.globalBools.Add(false); //LIGHTS ENABLED
            GameController.instance.globalBools.Add(false); //106 ENABLED
            GameController.instance.globalBools.Add(false); //PocketOnce

            GameController.instance.globalStrings.Add(getRandomString(4));//Maintenance code
            GameController.instance.globalStrings.Add(getRandomString(4));//Maynard code

            GameController.instance.globalFloats.Add(Random.Range(840, 1050)); //LARRY TIMER
            GameController.instance.globalFloats.Add(180); //AMBIANCETIMER
        }
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
        if (GameController.instance.doGameplay)
        {
            if(GameController.instance.currZone==0)
            {
                GameController.instance.globalBools[0] = true;
            }

            GameController.instance.globalFloats[0] -= Time.deltaTime;
            GameController.instance.globalFloats[1] -= Time.deltaTime;
            if (GameController.instance.globalFloats[0] < 0)
            {
                GameController.instance.globalFloats[0] = Random.Range(840, 1050); //LARRY TIMER
                Vector3 newpos = GameController.instance.player.transform.position;
                newpos.y -= 1f;
                GameController.instance.npcController.mainList[(int)npc.scp106].Spawn(true, newpos);
            }

            if (GameController.instance.globalFloats[1] < 0 && !ambiancechanged)
            {
                GameController.instance.DefaultAmbiance();
                ambiancechanged = true;
            }
        }
    }
}
