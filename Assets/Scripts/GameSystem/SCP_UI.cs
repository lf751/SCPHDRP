using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Menu { None, Pause, Inv , Death, Screen, Debug, Options, Keypad};

public class SCP_UI : MonoBehaviour
{

    public static SCP_UI instance = null;
    public Image eyes, eyegraphics, infectiongraphic;
    public Canvas PauseM;
    public Transform CanvasPos;
    public GameObject canvas, SNav, notifprefab;
    public Canvas Death, Screen, Options, Inventory;
    public Image ScreenText;
    public Canvas HUD;
    public EventSystem menu;
    public AudioClip[] inventory;
    public AudioClip menublip;
    public Text Info1, Info2, DeathMSG;
    public Button save;
    public RadioController radio;
    public KeypadController keypad;
    public 
    Menu currMenu = Menu.None;

    bool canConsole, canTuto, canPause;

    public Image Overlay, handEquip, navBar;
    public SegmentedSlider blinkBar, runBar;

    public GameObject defInv, defPause, hand;
    // Start is called before the first frame update

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
        canPause = false;
    }

    public void EnableMenu()
    {
        canPause = true;
    }

    public void ItemSFX(int sfx)
    {
        GameController.instance.MenuSFX.PlayOneShot(inventory[sfx]);
    }

    public void TogglePauseMenu()
    {
        if (currMenu == Menu.Pause)
        {
            SCPInput.instance.ToGameplay();
            GameController.instance.MenuSFX.PlayOneShot(menublip);
            PauseM.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1.0f;
            currMenu = Menu.None;
            AudioListener.pause = false;
            return;
        }
        if (currMenu == Menu.None&&canPause)
        {
            Info1.text = string.Format(Localization.GetString("uiStrings", "ui_in_info"), GlobalValues.design, GlobalValues.playername, GlobalValues.mapname, GlobalValues.mapseed);

            SCPInput.instance.ToUI();
            PauseM.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            currMenu = Menu.Pause;
            AudioListener.pause = true;
            return;
        }
    }

    public void ToggleOptionsMenu()
    {
        if (currMenu == Menu.Options)
        {
            GameController.instance.MenuSFX.PlayOneShot(menublip);
            Options.enabled = false;
            PauseM.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            currMenu = Menu.Pause;
            AudioListener.pause = true;
            GameController.instance.LoadUserValues();
            return;
        }
        if (currMenu == Menu.None || currMenu == Menu.Pause)
        {
            GameController.instance.MenuSFX.PlayOneShot(menublip);
            PauseM.enabled = false;
            Options.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            currMenu = Menu.Options;
            AudioListener.pause = true;
            return;
        }
    }

    public void ToggleInventory()
    {
        if (currMenu == Menu.Inv)
        {
            SCPInput.instance.ToGameplay();
            Inventory.enabled = false;
            Time.timeScale = 1.0f;
            ItemController.instance.CloseInv();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            currMenu = Menu.None;
            AudioListener.pause = false;
            return;
        }
        if (currMenu == Menu.None)
        {
            SCPInput.instance.ToUI();
            ItemController.instance.OpenInv();
            Inventory.enabled = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            currMenu = Menu.Inv;
            menu.SetSelectedGameObject(defInv);
            AudioListener.pause = true;

            return;
        }
    }

    public void ToggleDeath()
    {
        if (currMenu == Menu.Death)
        {
            SCPInput.instance.ToGameplay();
            DeathMSG.text = GameController.instance.deathmsg;
            Death.enabled = false;
            Time.timeScale = 1.0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            currMenu = Menu.None;
            return;
        }
        else
        {
            SCPInput.instance.ToUI();
            if (!GlobalValues.hasSaved)
                save.interactable = false;
            else
                save.interactable = true;
            Info2.text = string.Format(Localization.GetString("uiStrings", "ui_in_info"), GlobalValues.design, GlobalValues.playername, GlobalValues.mapname, GlobalValues.mapseed);
            DeathMSG.text = GameController.instance.deathmsg;
            Death.enabled = true;
            Time.timeScale = 1.0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            currMenu = Menu.Death;
            return;
        }
    }

    public void ToggleScreen()
    {
        if (currMenu == Menu.Screen)
        {
            Screen.enabled = false;

            GameController.instance.MenuSFX.PlayOneShot(menublip);
            /*Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;*/
            currMenu = Menu.None;
            return;
        }
        if (currMenu == Menu.None)
        {
            Screen.enabled = true;
            /*Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;*/
            currMenu = Menu.Screen;
            return;
        }
    }

    public void ToggleKeypad(Object_Keypad keypadObject)
    {
        if (currMenu == Menu.Keypad)
        {
            SCPInput.instance.ToGameplay();
            GameController.instance.MenuSFX.PlayOneShot(menublip);
            keypad.disableKeypad();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            /*GameController.instance.player.GetComponent<Player_Control>().Freeze = false;
            GameController.instance.player.GetComponent<Player_Control>().checkObjects = true;
            GameController.instance.player.GetComponent<Player_Control>().StopLook();*/
            currMenu = Menu.None;
            return;
        }
        if (currMenu == Menu.None)
        {
            SCPInput.instance.ToUI();
            keypad.enableKeypad(keypadObject);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            currMenu = Menu.Keypad;

            //SCP_UI.instance.ToggleScreen();
            /*GameController.instance.player.GetComponent<Player_Control>().Freeze = true;
            
            GameController.instance.player.GetComponent<Player_Control>().ForceLook(keypadObject.transform.position, 4f);*/
            return;
        }
    }

    public bool ToggleConsole()
    {
        if (canConsole)
        {
            if (currMenu == Menu.Debug)
            {
                SCPInput.instance.ToGameplay();
                Time.timeScale = 1.0f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                currMenu = Menu.None;
                return (false);
            }
            if (currMenu == Menu.None)
            {
                SCPInput.instance.ToUI();
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                currMenu = Menu.Debug;
                return (true);
            }
        }
        return (false);
    }


    public void LoadValues()
    {
        canConsole = (PlayerPrefs.GetInt("Debug", 0) == 1);
        canTuto = (PlayerPrefs.GetInt("Tutorials", 1) == 1);
    }



    public void ShowTutorial(string tuto)
    {
        if (canTuto)
        {
            GameObject notif = Instantiate(notifprefab, canvas.transform);
            NotifSystem notifval = notif.GetComponent<NotifSystem>();
            notifval.image.sprite = Resources.Load<Sprite>("Tutorials/" + tuto);
            notifval.body.text = Localization.GetString("tutoStrings", tuto);
        }
    }
}
