using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomHolder : MonoBehaviour
{
    public GameObject Room, Lights, Probes;
    public GameObject[] cutsceneReferences;
    public GameObject spawn;
    [System.NonSerialized]
    public Object_Door[] doors = new Object_Door[4];
}
