using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Datos para los caminantes
/// </summary>
/// 
[System.Serializable]
public class MapSize
{
    public int xSize, ySize;
}

[HideInInspector]
public class walker_dat
{
    public int x, y, dir, steps, lifespan;
    public bool primary;

    public walker_dat(int _x, int _y, int _lifespan, int _steps, int _dir, bool _primary)
    {
        x = _x;
        y = _y;
        lifespan = _lifespan;
        steps = _steps;
        dir = _dir;
        primary = _primary;

    }
};

public enum RoomType { TwoWay, FourWay, CornerWay, TWay, EndWay };
public enum Directions { Right, Down, Left, Up };

[HideInInspector]
[System.Serializable]
public class room_dat
{
    public RoomType type;
    public bool isSpecial, hasEvents = false, hasSpecial = false, hasItems = false, hasAmbiance = false;
    public float customFog;
    public GameObject RoomHolder;
    public int music = -1, Zone = 1;
};

[HideInInspector]
[System.Serializable]
public class room
{
    public RoomType type;
    public int angle, Event = -1, Zone, items = 0, EventState = 0;
    public bool empty = true, eventDone = false, isSpecial = false;
    public float customFog;
    public int[] neighbours = new int[4];
    public int[] values = new int[6] { 0, 0, 0, 0, 0, 0 };
    public string roomName;
};


[HideInInspector]
public class Roomlookup
{
    public int xPos, yPos;

    public Roomlookup(int _xPos, int _yPos)
    {
        xPos = _xPos;
        yPos = _yPos;
    }
}

[HideInInspector]
public class worldPos
{
    int x, y;
    public worldPos(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
}

class RoomCompare : IComparer<RoomChance>
{
    public int Compare(int x, int y)
    {
        if (x == 0 || y == 0)
        {
            return 0;
        }

        // CompareTo() method 
        return x.CompareTo(y);
    }

    public int Compare(RoomChance x, RoomChance y)
    {
        return x.Chance.CompareTo(y.Chance);
        //throw new System.NotImplementedException();
    }
}


public class NewMapGen : MonoBehaviour
{
    // Start is called before the first frame update
    public MapSize mapSize;
    public LayerMask ground;

    public Dictionary<string, room_dat> roomTable = new Dictionary<string, room_dat>();

    RoomList[] RoomTable;

    GameObject mapParent;

    public float roomsize;
    public int minHall, maxHall;

    List<walker_dat> walker_list = new List<walker_dat>();

    public int[,] mapgen;
    public int[,,] cull_lookup;
    public room[,] mapfil;
    public RoomHolder[,] mapobjects;
    public int zone3_limit;
    public int zone2_limit;
    private int mapdone;
    public string mapgenseed;
    public int extrawalker;
    bool IsNew = false;
    public bool CreationDone;
    public bool spawnspecial, useAlternateSpawn;

    int walker_count;
    public bool forceConnect, carving, carvtype1, carvtype2;
    readonly int[,] dirs = { { 1, 0 }, { 0, -1 }, { -1, 0 }, { 0, 1 } };

    GameObject twoway;
    GameObject fourway;
    GameObject tway;
    GameObject cornway;
    GameObject endway;

    public RoomList zEntrance;
    public RoomList zHeavy;
    public RoomList zLight;

    List<Roomlookup> cornerWay_Lookup = new List<Roomlookup>();
    List<Roomlookup> twoWay_Lookup = new List<Roomlookup>();
    List<Roomlookup> tWay_Lookup = new List<Roomlookup>();
    List<Roomlookup> endWay_Lookup = new List<Roomlookup>();
    List<Roomlookup> fourWay_Lookup = new List<Roomlookup>();

    public GameObject zone3Check;
    public GameObject zone2Check;
    public GameObject introRoom;

    public GameObject DoorLight, DoorHeavy;

    [HideInInspector]
    public List<RoomChance> speciallist;

    [HideInInspector]
    public List<string> eventList;

    void Awake()
    {
        ScanForSpecials(zEntrance.twoWay_List);
        ScanForSpecials(zEntrance.cornerWay_List);
        ScanForSpecials(zEntrance.endWay_List);
        ScanForSpecials(zEntrance.tWay_List);
        ScanForSpecials(zEntrance.fourWay_List);

        ScanForSpecials(zHeavy.twoWay_List);
        ScanForSpecials(zHeavy.cornerWay_List);
        ScanForSpecials(zHeavy.endWay_List);
        ScanForSpecials(zHeavy.tWay_List);
        ScanForSpecials(zHeavy.fourWay_List);

        ScanForSpecials(zLight.twoWay_List);
        ScanForSpecials(zLight.cornerWay_List);
        ScanForSpecials(zLight.endWay_List);
        ScanForSpecials(zLight.tWay_List);
        ScanForSpecials(zLight.fourWay_List);
    }

    void ScanForSpecials(List<RoomChance> roomlist)
    {
        int i;
        for (i = 0; i < (roomlist.Count); i++)
        {
            if (roomlist[i].isSpecial == true)
            {
                speciallist.Add(roomlist[i]);
            }
        }
        RoomCompare rc = new RoomCompare();

        roomlist.Sort(rc);
        AddToList(roomlist);
    }

    void AddToList(List<RoomChance> roomlist)
    {
        int i;
        for (i = 0; i < (roomlist.Count); i++)
        {
            room_dat temp = new room_dat();
            temp.RoomHolder = roomlist[i].Room;
            temp.music = roomlist[i].music;
            temp.isSpecial = roomlist[i].isSpecial;
            temp.hasSpecial = roomlist[i].hasSpecial;
            temp.hasEvents = roomlist[i].hasEvent;
            temp.hasItems = roomlist[i].hasItem;
            temp.Zone = roomlist[i].Zone;
            temp.type = roomlist[i].type;
            temp.hasAmbiance = roomlist[i].hasAmbiance;
            temp.customFog = roomlist[i].customFog;

            roomTable.Add(temp.RoomHolder.name, temp);
        }
    }

    private void MapStart()
    {
        eventList = new List<string>();
        eventList.Add("Porsiacaso");
        mapParent = new GameObject();
        mapParent.name = "Generated Map";


        mapgen = new int[mapSize.xSize, mapSize.ySize];
        if (IsNew)
            mapfil = new room[mapSize.xSize, mapSize.ySize];
        mapobjects = new RoomHolder[mapSize.xSize, mapSize.ySize];
        mapdone = 0;
        walker_count = 0;
    }





    public void CreaMundo()
    {
        IsNew = true;
        MapStart();
        Random.InitState(mapgenseed.GetHashCode());

        Debug.Log("Generating world");
        CreaLab();
        /*walker_list.Add(new walker_dat(mapSize.xSize / 2, zone3_limit-1, (mapSize.xSize/2)*3, minHall, 1, true));
        walker_list.Add(new walker_dat(mapSize.xSize / 2, zone2_limit-1, (mapSize.xSize / 2) * 3, minHall, 1, true));
        walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize - 2, mapSize.xSize, minHall, 0, true));
        walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize - 2, mapSize.xSize, minHall, 2, true));*/

        /*walker_list.Add(new walker_dat(mapSize.xSize / 2+4, zone3_limit - 1, mapSize.xSize, minHall, 1, true));
        walker_list.Add(new walker_dat(mapSize.xSize / 2-4, zone3_limit - 1, mapSize.xSize, minHall, 1, true));

        walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize / 2, (mapSize.xSize / 2) * 3, minHall, 1, true));
        walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize / 2, (mapSize.xSize / 2) * 3, minHall, 3, true));

        walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize - 2, mapSize.xSize, maxHall, 0, true));
        walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize - 2, mapSize.xSize, maxHall, 2, true));*/

        if (!useAlternateSpawn)
        {
            walker_list.Add(new walker_dat(mapSize.xSize / 2, 0, (mapSize.xSize / 2) * 3, minHall, (int)Directions.Up, true));

            walker_list.Add(new walker_dat(mapSize.xSize / 2, zone3_limit - 1, (mapSize.xSize / 2) * 3, minHall, 1, true));
            walker_list.Add(new walker_dat(mapSize.xSize / 2, zone2_limit + 1, (mapSize.xSize / 2) * 3, minHall, 3, true));

            walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize - 1, (mapSize.xSize / 2) * 3, minHall, (int)Directions.Down, true));
            //From left, 173 room
            walker_list.Add(new walker_dat(1, mapSize.ySize / 2, (mapSize.xSize / 2) * 2, minHall, 0, true));
            //walker_list.Add(new walker_dat(mapSize.xSize, mapSize.ySize / 2, (mapSize.xSize / 2) * 3, minHall, 2, true));
        }
        else
        {
            walker_list.Add(new walker_dat(mapSize.xSize / 2, mapSize.ySize - 1, (mapSize.xSize / 2) * 3, minHall, (int)Directions.Down, true));
            walker_list.Add(new walker_dat(mapSize.xSize / 2, 0, (mapSize.xSize / 2) * 3, minHall, (int)Directions.Up, true));

            walker_list.Add(new walker_dat(1, mapSize.ySize / 2, (mapSize.xSize / 2) * 3, maxHall, (int)Directions.Right, true));
            walker_list.Add(new walker_dat(mapSize.xSize - 1, mapSize.ySize / 2, (mapSize.xSize / 2) * 3, maxHall, (int)Directions.Left, true));

        }

        Step();

        if (forceConnect == true)
            ZoneConection();

        if (carving)
            UnionCarving();


        LlenarMundo();          //Al finalizar, llena el mapa con objetos

        mapfil[0, mapSize.ySize / 2] = new room();

        mapfil[0, mapSize.ySize / 2].roomName = introRoom.name;
        mapfil[0, mapSize.ySize / 2].isSpecial = true;
        mapfil[0, mapSize.ySize / 2].items = 1;
        mapfil[0, mapSize.ySize / 2].angle = 90;
        mapfil[0, mapSize.ySize / 2].Zone = 2;
        mapfil[0, mapSize.ySize / 2].type = RoomType.EndWay;
        mapfil[0, mapSize.ySize / 2].customFog = -1;

        if (spawnspecial)
            SpecialRoomSpawn();


        Debug.Log("Generated");
    }

    public room[,] DameMundo()
    {
        return (mapfil);
    }

    void CreaLab()          //Llena el arreglo del laberinto simple con 0
    {
        int i, j;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                mapgen[i, j] = 0;
            }
        }
    }



    void Step()
    {
        do
        {
            mapdone = 0;
            for (walker_count = 0; walker_count < walker_list.Count; walker_count++)
            {
                walker_list[walker_count] = Upda_walk(walker_list[walker_count]);
            }
        }
        while (mapdone != 0);
    }


    walker_dat Upda_walk(walker_dat temp)
    {
        mapdone += temp.lifespan;
        if (temp.lifespan != 0)
        {
            int tempNum, pathchance, newpath;

            temp.lifespan -= 1;

            if (!useAlternateSpawn)
            {
                if (temp.y == zone3_limit)
                {
                    temp.lifespan += 2;
                    temp.steps += 2;
                    temp.dir = 3;

                }
                if (temp.y == zone2_limit)
                {
                    temp.lifespan += 2;
                    temp.steps += 2;
                    temp.dir = 1;
                }
            }

            pathchance = Random.Range(0, 11);
            tempNum = temp.dir;           //Numero temporal para las direcciones
            if (temp.steps == 0)        //Si ya dejo de caminar
            {
                temp.steps = Random.Range(minHall, maxHall + 1);     //Obtiene un cantidad de pasos al azar que no sean 0
                tempNum = getdir(temp.dir);         //Obtiene una direccion aleatoria, y no es hacia atras de donde vino
                if (pathchance < extrawalker)
                {
                    newpath = countryroad(tempNum, temp.dir);
                    walker_list.Add(new walker_dat(temp.x, temp.y, (temp.lifespan / 2), minHall, newpath, false));
                    /*if (useAlternateSpawn && pathchance < 3)
                    {
                        walker_list.Add(new walker_dat(temp.x, temp.y, (temp.lifespan / 4), minHall, noatras(newpath), false));
                    }*/
                }
            }

            temp.dir = tempNum;

            while (atedge(temp.x, temp.y, temp.dir) == 1)     //Si al dar el siguiente paso sale de los limites del mundo
            {
                temp.dir = solveedge(temp.dir, tempNum);                 //Obtiene una nueva direccion que no la saque del mismo
                if (temp.primary == false)
                    temp.lifespan = 0;
            }
            temp.steps -= 1;
            temp.x += dirs[temp.dir, 0];                //Avanza la cantidad de coordenadas horizontales que le indique el arreglo de las direcciones
            temp.y += dirs[temp.dir, 1];                //Avanza la cantidad de coordenadas verticales que le indique el arreglo de las direcciones               //Disminuye los pasos
            mapgen[temp.x, temp.y] = 1;         //Abre un camino en el laberinto
        }
        else
        {
        }
        return (temp);
    }

    int solveedge(int newdir, int lastdir)
    {
        if (newdir != (lastdir + 1))
        {
            newdir += 1;
            if (newdir > 3)       //En caso de que al restar 2 quedemos en un numero negativo, sumamos 4, que tambien nos deja en la direccion contraria correcta
                newdir -= 4;
            return (newdir);
        }
        else
        {
            newdir -= 2;
            if (newdir < 0)       //En caso de que al restar 2 quedemos en un numero negativo, sumamos 4, que tambien nos deja en la direccion contraria correcta
                newdir += 4;
            return (newdir);
        }
    }

    int getdir(int dir)
    {
        int tempNum;
        int atras = noatras(dir);               //Obtiene la direccion contraria que no debe tomar
        do
        {
            tempNum = Random.Range(0, 4);
        }
        while (tempNum == atras || tempNum == dir);               //Obtendra numeros aleatorios hasta que dejen de coincidir con la direccion hacia atras
        return (tempNum);
    }


    int countryroad(int dir_a, int dir_b)
    {
        int tempNum;
        do
        {
            tempNum = Random.Range(0, 4);
        }
        while (tempNum == dir_a || tempNum == dir_b);               //Obtendra numeros aleatorios hasta que dejen de coincidir con la direccion hacia atras
        return (tempNum);
    }

    int atedge(int actX, int actY, int nMove)
    {
        if (((actX + dirs[nMove, 0]) == mapSize.xSize) || ((actX + dirs[nMove, 0]) == -1))     //Si al dar un paso de la posicion x que se encuentra, sale del mapa
        {
            //printf("dir a no tomar %i ", nMove);
            return (1);                                      //La funcion retorna 1, es decir, SI, estamos en la orilla
        }

        if (((actY + dirs[nMove, 1]) == mapSize.ySize) || ((actY + dirs[nMove, 1]) == -1))          //Si al dar un paso de la posicion y que se encuentra, sale del mapa
        {
            //printf("dir a no tomar %i ", nMove);
            return (1);
        }
        return (0);
    }

    int noatras(int diro)           //Calcula que direccion es la contraria a la que tomamos.
    {
        diro -= 2;                  //Ya que las direcciones estan ordenadas como "Derecha" "Arriba" "Izquierda" "Abajo", restar 2 , nos deja en la direccion contraria
        if (diro < 0)       //En caso de que al restar 2 quedemos en un numero negativo, sumamos 4, que tambien nos deja en la direccion contraria correcta
            diro += 4;
        return (diro);
    }

    void ZoneConection()
    {
        if (!useAlternateSpawn)
        {
            int i;
            for (i = zone3_limit - 1; i < zone2_limit + 1; i++)
            {
                /*if (mapgen[mapSize.xSize / 2, i] == 1)
                    break;
                else*/
                mapgen[mapSize.xSize / 2, i] = 1;
            }

            mapgen[0, mapSize.ySize / 2] = 1;
            mapgen[1, mapSize.ySize / 2] = 1;
            for (i = 2; i < mapSize.xSize; i++)
            {
                if (mapgen[i, mapSize.ySize / 2] == 1)
                    break;
                mapgen[i, mapSize.ySize / 2] = 1;
            }

            for (i = zone2_limit + 1; i > zone3_limit + 1; i--)
            {
                if (mapgen[mapSize.xSize / 2, i] == 1)
                    break;
                else
                    mapgen[mapSize.xSize / 2, i] = 1;
            }
        }
        else
        {
            int i;
            mapgen[mapSize.xSize / 2, 0] = 1;
            for (i = 0; i < (mapSize.ySize - 1); i++)
            {
                if (mapgen[(mapSize.xSize / 2) + 1, i] == 0 || mapgen[(mapSize.xSize / 2) + 1, i + 1] == 0)
                    mapgen[(mapSize.xSize / 2), i] = 1;

                if (mapgen[(mapSize.xSize / 2) + 1, i + 1] == 1)
                    mapgen[(mapSize.xSize / 2) + 1, i] = 1;

                if (mapgen[(mapSize.xSize / 2) - 1, i] == 0 || mapgen[(mapSize.xSize / 2) - 1, i + 1] == 0)
                    mapgen[(mapSize.xSize / 2), i] = 1;

                if (mapgen[(mapSize.xSize / 2) - 1, i + 1] == 1)
                    mapgen[(mapSize.xSize / 2) - 1, i] = 1;
            }

            mapgen[0, mapSize.ySize / 2] = 1;

            for (i = 1; i < (mapSize.ySize - 1); i++)
            {
                if (mapgen[i, (mapSize.ySize / 2) + 1] == 0 || mapgen[i + 1, (mapSize.ySize / 2) + 1] == 0)
                    mapgen[i, (mapSize.ySize / 2)] = 1;

                if (mapgen[i + 1, (mapSize.xSize / 2) + 1] == 1)
                    mapgen[i, (mapSize.xSize / 2) + 1] = 1;

                if (mapgen[i, (mapSize.ySize / 2) - 1] == 0 || mapgen[i + 1, (mapSize.ySize / 2) - 1] == 0)
                    mapgen[i, (mapSize.ySize / 2) - 1] = 1;

                if (mapgen[i + 1, (mapSize.xSize / 2) - 1] == 1)
                    mapgen[i, (mapSize.xSize / 2)] = 1;
            }
        }
    }


    void UnionCarving()          //Llena el laberinto con habitaciones aleatorias con objetos, enemigos, etc. Tambien le dice que cuartos tiene como vecinos
    {
        int i, j, delChance;

        mapgen[0, (mapSize.ySize / 2) - 1] = 0;

        if (mapgen[0, (mapSize.ySize / 2) + 1] == 1)
        {
            mapgen[0, (mapSize.ySize / 2) + 1] = 0;
            mapgen[0 + 1, (mapSize.ySize / 2) + 2] = 1;
        }

        if (mapgen[0, (mapSize.ySize / 2) + 2] == 1)
        {
            mapgen[0, (mapSize.ySize / 2) + 2] = 0;
            mapgen[0 + 1, (mapSize.ySize / 2) + 3] = 1;
        }


        /*if (mapgen[(mapSize.xSize / 2) - 1, mapSize.ySize - 1] == 1)
        /*{
            mapgen[(mapSize.xSize / 2) - 2, mapSize.ySize - 2] = 1;
            mapgen[(mapSize.xSize / 2) - 1, mapSize.ySize - 1] = 0;
        }

        mapgen[mapSize.xSize / 2+4, zone3_limit - 1] = 1;
        mapgen[mapSize.xSize / 2-4, zone3_limit - 1] = 1;
        mapgen[mapSize.xSize / 2 + 4, zone3_limit] = 1;
        mapgen[mapSize.xSize / 2 - 4, zone3_limit] = 1;
        mapgen[mapSize.xSize / 2, zone2_limit - 1] = 1;
        mapgen[mapSize.xSize / 2, zone3_limit] = 1;
        mapgen[mapSize.xSize / 2, mapSize.ySize / 2] = 1;
        mapgen[mapSize.xSize / 2, zone2_limit] = 1;

        mapgen[mapSize.xSize / 2, mapSize.ySize - 1] = 1;
        mapgen[mapSize.xSize / 2, mapSize.ySize - 2] = 1;*/

        ZoneCarving();


        for (i = 1; i < mapSize.xSize - 1; i++)
        {
            for (j = 0; j < mapSize.ySize - 1; j++)
            {
                if (mapgen[i, j] == 1 && carvtype1 == true)
                {
                    if ((mapgen[i + 1, j] == 1) && (mapgen[i + 1, j + 1] == 1) && (mapgen[i, j + 1] == 1) && (mapgen[i - 1, j + 1] == 1) && (mapgen[i - 1, j] == 1))
                    {
                        if (j != 0 && mapgen[i, j - 1] == 0)
                            mapgen[i, j] = 0;
                        else
                        if (j + 2 != mapSize.ySize && mapgen[i, j + 2] == 0)
                            mapgen[i, j + 1] = 0;
                    }
                    if (j != 0 && ((mapgen[i + 1, j] == 1) && (mapgen[i + 1, j + 1] == 1) && (mapgen[i, j + 1] == 1) && (mapgen[i, j - 1] == 1) && (mapgen[i + 1, j - 1] == 1)))
                    {
                        if (mapgen[i - 1, j] == 0)
                            mapgen[i, j] = 0;
                        else
                            if (i + 2 != mapSize.xSize && mapgen[i + 2, j] == 0)
                            mapgen[i + 1, j] = 0;
                    }
                }
            }
        }

        for (i = 1; i < mapSize.xSize - 1; i++)
        {
            for (j = 0; j < mapSize.ySize - 1; j++)
            {
                if (mapgen[i, j] == 1)
                {
                    if (j != 0 && i != mapSize.xSize - 1 && j != mapSize.ySize && carvtype2 == true)
                    {
                        if ((mapgen[i + 1, j] == 1) && (mapgen[i + 1, j + 1] == 1) && (mapgen[i, j + 1] == 1))
                        {
                            if (mapgen[i - 1, j] != 1 && mapgen[i, j - 1] != 1)
                            {
                                mapgen[i, j] = 0;
                            }
                            if (i != mapSize.xSize - 2 && (mapgen[i + 2, j] != 1 && mapgen[i + 1, j - 1] != 1))
                            {
                                mapgen[i + 1, j] = 0;
                            }
                            if (j != mapSize.ySize - 2 && (mapgen[i, j + 2] != 1 && mapgen[i - 1, j + 1] != 1))
                            {
                                mapgen[i, j + 1] = 0;
                            }
                            if (j != mapSize.ySize - 2 && i != mapSize.xSize - 2 && (mapgen[i + 2, j + 1] != 1 && mapgen[i + 1, j + 2] != 1))
                            {
                                mapgen[i + 1, j + 1] = 0;
                            }
                        }

                    }

                }
            }
        }

        for (i = 1; i < mapSize.xSize - 1; i++)
        {
            for (j = 1; j < mapSize.ySize; j++)
            {
                if (mapgen[i, j] == 1 && mapgen[i - 1, j] == 1 && mapgen[i + 1, j] == 1 && mapgen[i, j - 1] == 0 && mapgen[i - 1, j - 1] == 0 && mapgen[i + 1, j - 1] == 0)
                {
                    int rand = Random.Range(0, 2);
                    mapgen[i, j - 1] = rand;
                }

            }
        }

        for (i = 1; i < mapSize.xSize - 1; i++)
        {
            for (j = 0; j < mapSize.ySize - 1; j++)
            {
                if (mapgen[i, j] == 1 && mapgen[i - 1, j] == 1 && mapgen[i + 1, j] == 1 && mapgen[i, j + 1] == 0 && mapgen[i - 1, j + 1] == 0 && mapgen[i + 1, j + 1] == 0)
                {
                    int rand = Random.Range(0, 2);
                    mapgen[i, j + 1] = rand;
                }

            }

        }


    }

    void ZoneCarving()          //Llena el laberinto con habitaciones aleatorias con objetos, enemigos, etc. Tambien le dice que cuartos tiene como vecinos
    {
        int i;
        for (i = 0; i < mapSize.xSize - 1; i++)
        {
            if (mapgen[i + 1, zone3_limit] == 1)
            {
                //Debug.Log("Cutting room at x" + i + " Heavy to Entrance");
                mapgen[i, zone3_limit] = 0;
            }

            if (mapgen[i + 1, zone2_limit] == 1)
            {
                //Debug.Log("Cutting room at x" + i + " Light To Heavy");
                mapgen[i, zone2_limit] = 0;
                //Debug.Log(mapgen[i, zone2_limit] +" " + mapgen[i + 1, zone2_limit]);
            }
        }
    }




    void LlenarMundo()          //Llena el laberinto con habitaciones aleatorias con objetos, enemigos, etc. Tambien le dice que cuartos tiene como vecinos
    {
        int i, j;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if (mapgen[i, j] == 1)
                {
                    mapfil[i, j] = new room();
                    if ((atedge(i, j, 0) == 0) && (mapgen[i + 1, j] == 1))     //Si hay un cuarto a la derecha, se agregara a la lista de cuartos vecinos o colindantes
                    {
                        mapfil[i, j].neighbours[0] = 1;
                    }
                    else
                        mapfil[i, j].neighbours[0] = 0;   //Si no, se quedara como -1

                    if ((atedge(i, j, 1) == 0) && (mapgen[i, j - 1] == 1))
                    {
                        mapfil[i, j].neighbours[1] = 1;

                    }
                    else
                        mapfil[i, j].neighbours[1] = 0;
                    if ((atedge(i, j, 2) == 0) && (mapgen[i - 1, j] == 1))
                    {
                        mapfil[i, j].neighbours[2] = 1;

                    }
                    else
                        mapfil[i, j].neighbours[2] = 0;
                    if ((atedge(i, j, 3) == 0) && (mapgen[i, j + 1] == 1))
                    {
                        mapfil[i, j].neighbours[3] = 1;

                    }
                    else
                        mapfil[i, j].neighbours[3] = 0;

                    RoomCheck(i, j);
                }
                else
                    mapfil[i, j] = null;
            }
        }
    }

    public IEnumerator MostrarMundo()
    {
        int i, j;
        int roomsprocessed = -1;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if (mapgen[i, j] == 1)      //Imprime el mapa
                    RoomInstance(i, j);
                roomsprocessed += 1;
                //Debug.Log("Rooms " + roomsprocessed + " Total " + mapSize.xSize * mapSize.ySize + " percent " + ((0.5f / (mapSize.xSize * mapSize.ySize)) * roomsprocessed));
                if (!GlobalValues.debug)
                    LoadingSystem.instance.loadbar = 0.5f + (0.5f / (mapSize.xSize * mapSize.ySize)) * roomsprocessed;
            }
            yield return null;
        }
        DoorInstance();
        yield return null;

        //DecalSystem.instance.CombineDecals();
        Debug.Log("Mostrado");
        yield return null;
    }

    public int[,] MapaBinario()
    {
        return (mapgen);
    }

    void RoomCheck(int i, int j)
    {
        bool cantSpawn = true;
        if (j < zone3_limit)
            mapfil[i, j].Zone = 1;

        if (j > zone3_limit && j < zone2_limit)
            mapfil[i, j].Zone = 2;
        if (j > zone2_limit)
            mapfil[i, j].Zone = 3;

        if (j == zone3_limit && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            mapfil[i, j].roomName = zone3Check.name;
            mapfil[i, j].angle = 0;
            mapfil[i, j].type = RoomType.TwoWay;
            mapfil[i, j].isSpecial = true;
            mapfil[i, j].Zone = 0;
            mapfil[i, j].customFog = -1;
            cantSpawn = false;

            twoWay_Lookup.Add(new Roomlookup(i, j));
            return;
        }
        if (j == zone2_limit && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            mapfil[i, j].roomName = zone2Check.name;
            mapfil[i, j].angle = 0;
            mapfil[i, j].type = RoomType.TwoWay;
            mapfil[i, j].isSpecial = true;
            mapfil[i, j].Zone = 0;
            mapfil[i, j].customFog = -1;
            twoWay_Lookup.Add(new Roomlookup(i, j));
            cantSpawn = false;
            return;
        }

        if (j == zone3_limit && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[3] == 1)
        {
            RoomSpawn(zHeavy.endWay_List, 0, RoomType.EndWay, i, j);
            mapfil[i, j].Zone = 2;
            cantSpawn = false;
            return;
        }

        if (j == zone3_limit && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[3] == 0)
        {
            RoomSpawn(zEntrance.endWay_List, 180, RoomType.EndWay, i, j);
            mapfil[i, j].Zone = 1;
            cantSpawn = false;
            return;
        }

        if (j == zone2_limit && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[3] == 1)
        {
            RoomSpawn(zLight.endWay_List, 0, RoomType.EndWay, i, j);
            mapfil[i, j].Zone = 3;
            cantSpawn = false;
            return;
        }

        if (j == zone2_limit && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[3] == 0)
        {
            RoomSpawn(zHeavy.endWay_List, 180, RoomType.EndWay, i, j);
            mapfil[i, j].Zone = 2;
            cantSpawn = false;
            return;
        }

        if (cantSpawn && (j == zone2_limit || j == zone3_limit))
        {
            mapgen[i, j] = 0;
            mapfil[i, j] = null;
            return;
        }




        ////////Pasillo

        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.twoWay_List, 90, RoomType.TwoWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.twoWay_List, 90, RoomType.TwoWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.twoWay_List, 90, RoomType.TwoWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.twoWay_List, 0, RoomType.TwoWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.twoWay_List, 0, RoomType.TwoWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.twoWay_List, 0, RoomType.TwoWay, i, j);
            return;
        }




        ///////////////////////////Corner Way

        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.cornerWay_List, 180, RoomType.CornerWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.cornerWay_List, 180, RoomType.CornerWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.cornerWay_List, 180, RoomType.CornerWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.cornerWay_List, -90, RoomType.CornerWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.cornerWay_List, -90, RoomType.CornerWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.cornerWay_List, -90, RoomType.CornerWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.cornerWay_List, 0, RoomType.CornerWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.cornerWay_List, 0, RoomType.CornerWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.cornerWay_List, 0, RoomType.CornerWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.cornerWay_List, 90, RoomType.CornerWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.cornerWay_List, 90, RoomType.CornerWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.cornerWay_List, 90, RoomType.CornerWay, i, j);
            return;
        }



        ///////////////TWay

        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.tWay_List, 0, RoomType.TWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.tWay_List, 0, RoomType.TWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.tWay_List, 0, RoomType.TWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.tWay_List, -90, RoomType.TWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.tWay_List, -90, RoomType.TWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.tWay_List, -90, RoomType.TWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.tWay_List, 180, RoomType.TWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.tWay_List, 180, RoomType.TWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.tWay_List, 180, RoomType.TWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.tWay_List, 90, RoomType.TWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.tWay_List, 90, RoomType.TWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.tWay_List, 90, RoomType.TWay, i, j);
            return;
        }


        /////////////////////////EndWay
        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit || j == zone3_limit || j == zone2_limit)
                RoomSpawn(zEntrance.endWay_List, 90, RoomType.EndWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.endWay_List, 90, RoomType.EndWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.endWay_List, 90, RoomType.EndWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit || j == zone3_limit || j == zone2_limit)
                RoomSpawn(zEntrance.endWay_List, 180, RoomType.EndWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.endWay_List, 180, RoomType.EndWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.endWay_List, 180, RoomType.EndWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 0)
        {
            if (j < zone3_limit || j == zone3_limit || j == zone2_limit)
                RoomSpawn(zEntrance.endWay_List, -90, RoomType.EndWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.endWay_List, -90, RoomType.EndWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.endWay_List, -90, RoomType.EndWay, i, j);
            return;
        }
        if (mapfil[i, j].neighbours[0] == 0 && mapfil[i, j].neighbours[1] == 0 && mapfil[i, j].neighbours[2] == 0 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit || j == zone3_limit || j == zone2_limit)
                RoomSpawn(zEntrance.endWay_List, 0, RoomType.EndWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.endWay_List, 0, RoomType.EndWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.endWay_List, 0, RoomType.EndWay, i, j);
            return;
        }

        ////////////////////////////////////////////////////////////

        if (mapfil[i, j].neighbours[0] == 1 && mapfil[i, j].neighbours[1] == 1 && mapfil[i, j].neighbours[2] == 1 && mapfil[i, j].neighbours[3] == 1)
        {
            if (j < zone3_limit)
                RoomSpawn(zEntrance.fourWay_List, 0, RoomType.FourWay, i, j);
            if (j > zone3_limit && j < zone2_limit)
                RoomSpawn(zHeavy.fourWay_List, 0, RoomType.FourWay, i, j);
            if (j > zone2_limit)
                RoomSpawn(zLight.fourWay_List, 0, RoomType.FourWay, i, j);
            return;
        }

        mapgen[i, j] = 0;
        mapfil[i, j] = null;
        return;
    }

    void RoomSpawn(List<RoomChance> roomlist, int angle, RoomType type, int i, int j)
    {
        int z, gend, maxvalue = 0;
        for (z = 0; z < (roomlist.Count); z++)
        {
            maxvalue += roomlist[z].Chance;
        }

        gend = Random.Range(0, maxvalue);

        for (z = 0; z < roomlist.Count; z++)
        {
            if (gend < roomlist[z].Chance)
            {
                /*mapfil[i, j].roomName = roomlist[z].Room.name;
                mapfil[i, j].angle = angle;
                mapfil[i, j].type = type;

                if (roomlist[z].hasItem)
                    mapfil[i, j].items = 1;

                if (roomlist[z].hasAmbiance)
                    mapfil[i, j].hasAmbiance= true;*/

                mapfil[i, j].angle = angle;

                mapfil[i, j] = SetRoomValues(roomlist[z], mapfil[i, j]);


                switch (type)
                {
                    case RoomType.TwoWay:
                        twoWay_Lookup.Add(new Roomlookup(i, j));
                        break;
                    case RoomType.TWay:
                        tWay_Lookup.Add(new Roomlookup(i, j));
                        break;
                    case RoomType.CornerWay:
                        cornerWay_Lookup.Add(new Roomlookup(i, j));
                        break;
                    case RoomType.EndWay:
                        endWay_Lookup.Add(new Roomlookup(i, j));
                        break;
                    case RoomType.FourWay:
                        fourWay_Lookup.Add(new Roomlookup(i, j));
                        break;
                }
                //Debug.Log("Cuarto " + mapfil[i, j].roomName + "Posicion " + i + " " + j);
                return;
            }
            else
                gend -= roomlist[z].Chance;
        }
    }

    room SetRoomValues(RoomChance data, room temp)
    {
        temp.roomName = data.Room.name;
        temp.type = data.type;
        temp.customFog = data.customFog;

        if (data.hasItem)
            temp.items = 1;
        else temp.items = 0;


        return (temp);
    }

    void SpecialRoomSpawn()
    {
        int i, chance, tries = 10000;
        ref List<Roomlookup> currtype = ref twoWay_Lookup;
        bool spawned;
        for (i = 0; i < (speciallist.Count); i++)
        {
            tries = 10000;
            spawned = false;
            switch (speciallist[i].type)
            {
                case RoomType.TwoWay:
                    {
                        currtype = ref twoWay_Lookup;
                        break;
                    }
                case RoomType.CornerWay:
                    {
                        currtype = ref cornerWay_Lookup;
                        break;
                    }
                case RoomType.TWay:
                    {
                        currtype = ref tWay_Lookup;
                        break;
                    }
                case RoomType.EndWay:
                    {
                        currtype = ref endWay_Lookup;
                        break;
                    }
                case RoomType.FourWay:
                    {
                        currtype = ref fourWay_Lookup;
                        break;
                    }
                default:
                    {
                        currtype = ref endWay_Lookup;
                        break;
                    }
            }

            List<Roomlookup> candidates = new List<Roomlookup>();

            for (int j = 0; j < currtype.Count; j++)
            {
                if ((mapfil[currtype[j].xPos, currtype[j].yPos].isSpecial == false) && mapfil[currtype[j].xPos, currtype[j].yPos].Zone == speciallist[i].Zone)
                {
                    candidates.Add(currtype[j]);
                }
            }


            if (candidates.Count == 0)
            {
                Debug.Log("EMPTY LOOKUP TABLE AT " + speciallist[i].type);
                //throw new System.Exception(v);
                continue;
            }

            //throw new System.Exception("EMPTY LOOKUP TABLE AT " + speciallist[i].type);
            chance = Random.Range(0, candidates.Count);
            mapfil[candidates[chance].xPos, candidates[chance].yPos] = SetRoomValues(speciallist[i], mapfil[candidates[chance].xPos, candidates[chance].yPos]);
            mapfil[candidates[chance].xPos, candidates[chance].yPos].isSpecial = true;
            spawned = true;
        }
    }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    /*void SavedtoMap(int i, int j)
    {
        string ZonePath = "";
        switch (mapsave[i, j].Zone)
        {
            case 1:
                {
                    ZonePath = "Map/Office/";
                        break;
                }
            case 2:
                {
                    ZonePath = "Map/Light/";
                    break;
                }
            case 3:
                {
                    ZonePath = "Map/Heavy/";
                    break;
                }
            case 0:
                {
                    ZonePath = "Map/CheckPoints/";
                    break;
                }
        }

        if (j == zone2_limit && mapsave[i, j].Zone != 0)
            ZonePath = "Map/Office/";
        if (j == zone3_limit && mapsave[i, j].Zone != 0)
            ZonePath = "Map/Heavy/";


        mapfil[i, j].RoomHolder = Resources.Load<GameObject>(string.Concat(ZonePath, mapsave[i, j].roomName));
        Debug.Log("Armando " + string.Concat(ZonePath, mapsave[i, j].roomName) + "En " + i + " " +j);
        mapfil[i, j].angle = mapsave[i, j].angle;
        mapfil[i, j].empty = mapsave[i, j].empty;
        mapfil[i, j].type = RoomType.TwoWay;
        mapfil[i, j].eventPreset = true;
        mapfil[i, j].hasItem = mapsave[i, j].items;

        mapfil[i, j].Event = mapsave[i, j].Event;
        mapfil[i, j].eventDone = false;

        if (mapfil[i, j].Event >= 0)
            mapfil[i, j].hasEvents = true;
        if (mapfil[i, j].Event == -2)
            mapfil[i, j].hasSpecial = true;
    }*/


    public void LoadingSave()
    {
        int i, j;

        MapStart();

        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if (mapfil[i, j] != null)
                {
                    mapgen[i, j] = 1;
                }
                else
                    mapgen[i, j] = 0;
            }
        }
        Debug.Log("Loaded");
    }









    void RoomInstance(int i, int j)
    {

        //Debug.Log("Creating " + mapfil[i, j].roomName + " in " + i + " " + j);

        room_dat room_ = roomTable[mapfil[i, j].roomName];

        int angleTemp = mapfil[i, j].angle;
        mapobjects[i, j] = Instantiate(room_.RoomHolder, new Vector3(roomsize * i, 0.0f, roomsize * j), Quaternion.Euler(0, angleTemp, 0)).GetComponent<RoomHolder>();
        mapobjects[i, j].transform.parent = mapParent.transform;


        int randomDecalAmm = Random.Range(0, 3);
        for (int dec = 0; dec < randomDecalAmm; dec++)
        {
            float xDecal = Random.Range(0, roomsize) - (roomsize / 2);
            float yDecal = Random.Range(0, roomsize) - (roomsize / 2);
            if (Physics.CheckSphere(new Vector3((roomsize * i) + xDecal, 0f, (roomsize * j) + yDecal), 0.1f, ground, QueryTriggerInteraction.Ignore))
                DecalSystem.instance.SpawnDecal(new Vector3((roomsize * i) + xDecal, 0f, (roomsize * j) + yDecal));
        }


        if (mapfil[i, j].Event == -1 && !SaveSystem.instance.playData.worldsCreateds[(int)GameController.ins.worldName])
        {
            if (room_.hasEvents)
            {
                mapfil[i, j].Event = mapobjects[i, j].GetComponent<EventHandler>().EventSet();
            }
            else
                mapfil[i, j].Event = -1;

            /////////////////////////////////////////////////////////////////////
            if (room_.hasSpecial)
            {
                bool found = false;
                string evName = mapobjects[i, j].GetComponent<EventHandler>().GetEventName();

                for (int z = 0; z < eventList.Count; z++)
                {
                    //Debug.Log(eventList[z]);
                    if (eventList[z] == evName)
                        found = true;
                }

                if (!found)
                {
                    eventList.Add(evName);
                    //Debug.Log("Agregando " + evName);
                    mapobjects[i, j].GetComponent<EventHandler>().EventSpecial();
                    mapfil[i, j].Event = -2;
                }
            }
        }
        else
        {
            if (room_.hasEvents || room_.hasSpecial)
                mapobjects[i, j].GetComponent<EventHandler>().ForceEvent(mapfil[i, j].Event);
        }
    }

    void DoorInstance()
    {
        int i, j;
        GameObject doorhold;
        for (i = 0; i < mapSize.xSize; i++)
        {
            for (j = 0; j < mapSize.ySize; j++)
            {
                if (i != mapSize.xSize - 1)
                {
                    if (mapgen[i, j] == 1 && mapgen[i + 1, j] == 1)
                    {
                        if (j < zone2_limit)
                            doorhold = Instantiate(DoorLight, new Vector3(roomsize * i + (roomsize / 2), 0.0f, roomsize * j), Quaternion.identity * Quaternion.Euler(0, 90, 0));
                        else
                            doorhold = Instantiate(DoorHeavy, new Vector3(roomsize * i + (roomsize / 2), 0.0f, roomsize * j), Quaternion.identity * Quaternion.Euler(0, 90, 0));

                        mapobjects[i, j].doors[(int)Directions.Right] = doorhold.GetComponentInChildren<Object_Door>(true);
                        mapobjects[i + 1, j].doors[(int)Directions.Left] = doorhold.GetComponentInChildren<Object_Door>(true);
                        doorhold.transform.parent = GameController.ins.doorParent.transform;
                    }
                }
                if (j != mapSize.ySize - 1)
                {
                    if (mapgen[i, j] == 1 && mapgen[i, j + 1] == 1)
                    {
                        if (j < zone2_limit)
                            doorhold = Instantiate(DoorLight, new Vector3(roomsize * i, 0.0f, roomsize * j + (roomsize / 2)), Quaternion.identity);
                        else
                            doorhold = Instantiate(DoorHeavy, new Vector3(roomsize * i, 0.0f, roomsize * j + (roomsize / 2)), Quaternion.identity);

                        mapobjects[i, j].doors[(int)Directions.Up] = doorhold.GetComponentInChildren<Object_Door>(true);
                        mapobjects[i, j + 1].doors[(int)Directions.Down] = doorhold.GetComponentInChildren<Object_Door>(true);

                        doorhold.transform.parent = GameController.ins.doorParent.transform;
                    }
                }
            }
        }
    }
}