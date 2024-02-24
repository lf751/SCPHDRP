using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Menu { None, Pause, Inv, Death, Screen, Debug, Options, Keypad };

public class SCP_UI : MonoBehaviour
{

    public static SCP_UI instance = null;
    public Image eyes, eyegraphics, infectiongraphic;
    public GameObject NotifPrefab, Canvas, SNav;
    public Canvas Inventory, Death, Screen, Options, PauseM, HUD;
    public Image ScreenText;
    public EventSystem menu;
    public AudioClip[] inventory;
    public AudioClip menublip;
    public Text Info1, Info2, DeathMSG;
    public Button save;
    public RadioController radio;
    public KeypadController keypad;
    public Menu currMenu = Menu.None;

    bool canConsole, canTuto, canPause;

    public Image blinkBar, Overlay, handEquip, runBar, navBar;

    public GameObject defInv, defPause, hand;
    public HashSet<GameObject> loadedEquips;
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
        GameController.ins.MenuSFX.PlayOneShot(inventory[sfx]);
    }

    public void TogglePauseMenu()
    {
        if (currMenu == Menu.Pause)
        {
            SCPInput.instance.ToGameplay();
            GameController.ins.MenuSFX.PlayOneShot(menublip);
            PauseM.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1.0f;
            currMenu = Menu.None;
            AudioListener.pause = false;
            return;
        }
        if (currMenu == Menu.None && canPause)
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
            GameController.ins.MenuSFX.PlayOneShot(menublip);
            Options.enabled = false;
            PauseM.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            currMenu = Menu.Pause;
            AudioListener.pause = true;
            GameController.ins.LoadUserValues();
            return;
        }
        if (currMenu == Menu.None || currMenu == Menu.Pause)
        {
            GameController.ins.MenuSFX.PlayOneShot(menublip);
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
            Inventory.enabled = true;
            ItemController.instance.OpenInv();
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
            DeathMSG.text = GameController.ins.deathmsg;
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
            DeathMSG.text = GameController.ins.deathmsg;
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
            hand.SetActive(true);
            GameController.ins.MenuSFX.PlayOneShot(menublip);
            /*Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;*/
            currMenu = Menu.None;
            return;
        }
        if (currMenu == Menu.None)
        {
            hand.SetActive(false);
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
            GameController.ins.MenuSFX.PlayOneShot(menublip);
            keypad.disableKeypad();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            hand.SetActive(true);
            /*GameController.instance.player.GetComponent<Player_Control>().Freeze = false;
            GameController.instance.player.GetComponent<Player_Control>().checkObjects = true;
            GameController.instance.player.GetComponent<Player_Control>().StopLook();*/
            currMenu = Menu.None;
            return;
        }
        if (currMenu == Menu.None)
        {
            SCPInput.instance.ToUI();
            hand.SetActive(false);
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
                AudioListener.pause = false;
                return (false);
            }
            if (currMenu == Menu.None)
            {
                SCPInput.instance.ToUI();
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                currMenu = Menu.Debug;
                AudioListener.pause = true;
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
            GameObject notif = Instantiate(NotifPrefab, Canvas.transform);
            NotifSystem notifval = notif.transform.GetChild(0).GetComponent<NotifSystem>();
            notifval.image.sprite = Resources.Load<Sprite>("Tutorials/" + tuto);
            notifval.body.text = Localization.GetString("tutoStrings", tuto);
        }
    }
}
