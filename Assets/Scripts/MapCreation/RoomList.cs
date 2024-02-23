using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomChance
{
    public GameObject Room;
    public int Chance;
    public bool isSpecial, hasEvent, hasSpecial, hasItem = false, hasAmbiance=false;
    public float customFog = -1;
    public RoomType type;
    public int Zone;
    public int music = -1;
        public RoomChance(RoomType _type)
        {
        type = _type;
        }
}

[CreateAssetMenu(fileName = "new RoomTable", menuName = "Map Creator/Room Table")]
public class RoomList : ScriptableObject
{
    public int Zone;
    //This is our list we want to use to represent our class as an array.
    public List<RoomChance> twoWay_List = new(1);
    public List<RoomChance> cornerWay_List = new(1);
    public List<RoomChance> tWay_List = new(1);
    public List<RoomChance> endWay_List = new(1);
    public List<RoomChance> fourWay_List = new(1);


    public void AddNew(RoomType choiceList)
    {
        switch (choiceList)
        {
            case RoomType.TwoWay:
                {
                    twoWay_List.Add(new RoomChance( choiceList));
                    break;
                }
            case RoomType.CornerWay:
                {
                    cornerWay_List.Add(new RoomChance(choiceList));
                    break;
                }
            case RoomType.TWay:
                {
                    tWay_List.Add(new RoomChance(choiceList));
                    break;
                }
            case RoomType.EndWay:
                {
                    endWay_List.Add(new RoomChance(choiceList));
                    break;
                }
            case RoomType.FourWay:
                {
                    fourWay_List.Add(new RoomChance(choiceList));
                    break;
                }
        }
    }

    public void InsertNew(RoomType choiceList, int i)
    {
        switch (choiceList)
        {
            case RoomType.TwoWay:
                {
                    twoWay_List.Insert(i, new RoomChance(choiceList));
                    break;
                }
            case RoomType.CornerWay:
                {
                    cornerWay_List.Insert(i, new RoomChance(choiceList));
                    break;
                }
            case RoomType.TWay:
                {
                    tWay_List.Insert(i, new RoomChance(choiceList));
                    break;
                }
            case RoomType.EndWay:
                {
                    endWay_List.Insert(i, new RoomChance(choiceList));
                    break;
                }
            case RoomType.FourWay:
                {
                    fourWay_List.Insert(i, new RoomChance(choiceList));
                    break;
                }
        }
    }

    public void Remove(int index, RoomType choiceList)
    {
        //Remove an index position from our list at a point in our list array
            switch (choiceList)
            {
            case RoomType.TwoWay:
                {
                    twoWay_List.RemoveAt(index);
                    break;
                    }
            case RoomType.CornerWay:
                {
                    cornerWay_List.RemoveAt(index);
                    break;
                    }
            case RoomType.TWay:
                {
                    tWay_List.RemoveAt(index);
                    break;
                }
            case RoomType.EndWay:
                {
                    endWay_List.RemoveAt(index);
                    break;
                }
            case RoomType.FourWay:
                {
                    fourWay_List.RemoveAt(index);
                    break;
                }
        }
    }
}
