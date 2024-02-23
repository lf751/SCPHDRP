using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetUIString : MonoBehaviour
{
    public string StringCode;
    public string Stringtable = "uiStrings";
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Text>().text = Localization.GetString(Stringtable,StringCode);
    }

}
