using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LoadType {newgame, loadgame, mapless, otherworld };

public static class GlobalValues
{

    //Game Values
    static public LoadType LoadType = LoadType.newgame;
    static public string mapseed = "pepino";
    static public string mapname = "melapelas";
    static public string design = "pepino";
    static public string playername = "melapelas";

    const string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";

    static public int renderTime = 0;
    static public bool debug = true;
    static public bool debugconsole = false;
    static public bool isNewGame = true;
    static public bool hasSaved = false;
    static public bool playIntro = false;
    static public string pathfile;
    static public SaveData worldState;
    public const string folderName = "CBSaves";
    public const string localName = "Language";
    public const string fileExtension = ".scp";
    public const string metaExtension = ".meta";
    static public int sceneReturn = -1;
    static public string saveFileVer = "0.3.1";

    static public string getRandomString(int min, int max)
    {
        string myString = "";
        int charAmount = Random.Range(min, max); //set those to the minimum and maximum length of your string
        for (int i = 0; i < charAmount; i++)
        {
            myString += glyphs[Random.Range(0, glyphs.Length)];
        }
        return myString;
    }

    public static readonly int[] sceneTable = new int[3] { 1, 3, 6};


}