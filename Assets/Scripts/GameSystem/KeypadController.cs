using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeypadController : MonoBehaviour
{

    public GameObject keypad;
    Object_Keypad objectKeypad;
    public Text codeDisplay;
    string currentCode = "";
    public AudioClip buttonClick;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    /*// Update is called once per frame
    void Update()
    {
        
    }*/

    public void enableKeypad(Object_Keypad Keypad)
    {
        objectKeypad = Keypad;
        codeDisplay.text = "";
        currentCode = "";
        keypad.SetActive(true);
    }

    public void disableKeypad()
    {
        objectKeypad = null;
        keypad.SetActive(false);
    }



    public void AddDigit(string digit)
    {
        GameController.instance.MenuSFX.PlayOneShot(buttonClick);
        if (currentCode.Length < 4)
        {
            currentCode += digit;
            codeDisplay.text = currentCode;
        }
    }

    public void ClearDigits()
    {
        GameController.instance.MenuSFX.PlayOneShot(buttonClick);
        codeDisplay.text = "";
        currentCode = "";
    }

    public void SubmitCode()
    {
        GameController.instance.MenuSFX.PlayOneShot(buttonClick);
        objectKeypad.checkCode(currentCode);
        //SCP_UI.instance.ToggleKeypad(null);
    }



}
