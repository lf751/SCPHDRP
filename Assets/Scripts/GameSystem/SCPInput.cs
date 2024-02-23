using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum gameplayActions { MoveVector, ViewVector, MoveU, MoveD, MoveL, MoveR, Blink, Run, ActionMain, ActionHold, ActionCancel, Inventory, Pause, Save, Crouch, RadioNext, RadioPast }

public delegate void OnRebindOver();
public class SCPInput : MonoBehaviour
{
    /// <summary>
    /// Private wrapper class for json serialization of the overrides
    /// </summary>
    [System.Serializable]
    class BindingWrapperClass
    {
        public List<BindingSerializable> bindingList = new List<BindingSerializable>();
    }

    /// <summary>
    /// internal struct to store an id overridepath pair for a list
    /// </summary>
    [System.Serializable]
    private struct BindingSerializable
    {
        public string id;
        public string path;

        public BindingSerializable(string bindingId, string bindingPath)
        {
            id = bindingId;
            path = bindingPath;
        }
    }

    public DefaultActions playerInput;
    public static SCPInput instance;
    public Text inputText;
    public GameObject rebindScreen;

    public event OnRebindOver onRebindOver;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            playerInput = new DefaultActions();
            LoadRebinds();
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            DestroyImmediate(this.gameObject);
        }
    }

    public void ToGameplay()
    {
        //playerInput.Gameplay.Enable();
    }
    public void ToUI()
    {
        //playerInput.Gameplay.Disable();
    }

    public void TestRebind()
    {
        //RemapButtonClicked(playerInput.Gameplay.Blink);
    }

    public string GetBindName(gameplayActions actionToRebind, bool isAlt)
    {
        switch (actionToRebind)
        {
            case gameplayActions.ViewVector:
                {
                    return playerInput.Gameplay.Look.bindings[1].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.MoveVector:
                {
                    return playerInput.Gameplay.Move.bindings[5].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.MoveU:
                {
                    return playerInput.Gameplay.Move.bindings[1].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.MoveD:
                {
                    return playerInput.Gameplay.Move.bindings[2].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.MoveL:
                {
                    return playerInput.Gameplay.Move.bindings[3].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.MoveR:
                {
                    return playerInput.Gameplay.Move.bindings[4].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.ActionMain:
                {
                    return playerInput.Gameplay.InteractYes.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.ActionCancel:
                {
                    return playerInput.Gameplay.InteractNo.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.ActionHold:
                {
                    return playerInput.Gameplay.InteractHold.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.Blink:
                {
                    return playerInput.Gameplay.Blink.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.Crouch:
                {
                    return playerInput.Gameplay.CrouchTrigger.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.Run:
                {
                    return playerInput.Gameplay.RunHold.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.Inventory:
                {
                    return playerInput.Gameplay.Inventory.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.Save:
                {
                    return playerInput.Gameplay.Save.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.Pause:
                {
                    return playerInput.Gameplay.Pause.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.RadioNext:
                {
                    return playerInput.Gameplay.RadioNext.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            case gameplayActions.RadioPast:
                {
                    return playerInput.Gameplay.RadioBack.bindings[isAlt ? 1 : 0].ToDisplayString(InputBinding.DisplayStringOptions.DontOmitDevice);
                    break;
                }
            default:
                {
                    return "what";
                    break;
                }
        }
    }

    public void DoRebind(gameplayActions actionToRebind, bool isAlt)
    {
        switch (actionToRebind)
        {
            case gameplayActions.MoveU:
                {
                    RemapButtonClicked(playerInput.Gameplay.Move, false, "Button", 1);
                    break;
                }
            case gameplayActions.MoveD:
                {
                    RemapButtonClicked(playerInput.Gameplay.Move, false, "Button", 2);
                    break;
                }
            case gameplayActions.MoveL:
                {
                    RemapButtonClicked(playerInput.Gameplay.Move, false, "Button", 3);
                    break;
                }
            case gameplayActions.MoveR:
                {
                    RemapButtonClicked(playerInput.Gameplay.Move, false, "Button", 4);
                    break;
                }
            case gameplayActions.MoveVector:
                {
                    RemapButtonClicked(playerInput.Gameplay.Move, false, "Vector2", 5);
                    break;
                }
            case gameplayActions.ViewVector:
                {
                    RemapButtonClicked(playerInput.Gameplay.Look, false, "Vector2", 1);
                    break;
                }
            case gameplayActions.ActionMain:
                {
                    RemapButtonClicked(playerInput.Gameplay.InteractYes, isAlt, "Button");
                    break;
                }
            case gameplayActions.ActionCancel:
                {
                    RemapButtonClicked(playerInput.Gameplay.InteractNo, isAlt, "Button");
                    break;
                }
            case gameplayActions.ActionHold:
                {
                    RemapButtonClicked(playerInput.Gameplay.InteractHold, isAlt, "Button");
                    break;
                }
            case gameplayActions.Blink:
                {
                    RemapButtonClicked(playerInput.Gameplay.Blink, isAlt, "Button");
                    break;
                }
            case gameplayActions.Crouch:
                {
                    RemapButtonClicked(playerInput.Gameplay.CrouchTrigger, isAlt, "Button");
                    break;
                }
            case gameplayActions.Run:
                {
                    RemapButtonClicked(playerInput.Gameplay.RunHold, isAlt, "Button");
                    break;
                }
            case gameplayActions.Inventory:
                {
                    RemapButtonClicked(playerInput.Gameplay.Inventory, isAlt, "Button");
                    break;
                }
            case gameplayActions.Save:
                {
                    RemapButtonClicked(playerInput.Gameplay.Save, isAlt, "Button");
                    break;
                }
            case gameplayActions.Pause:
                {
                    RemapButtonClicked(playerInput.Gameplay.Pause, isAlt, "Button");
                    break;
                }
            case gameplayActions.RadioNext:
                {
                    RemapButtonClicked(playerInput.Gameplay.RadioNext, isAlt, "Button");
                    break;
                }
            case gameplayActions.RadioPast:
                {
                    RemapButtonClicked(playerInput.Gameplay.RadioBack, isAlt, "Button");
                    break;
                }
        }
    }

    void RemapButtonClicked(InputAction actionToRebind, bool isAlt, string controlType, int specificTarget = -1)
    {
        int target = (specificTarget == -1 ? (isAlt ? 1 : 0) : specificTarget);
        Debug.Log("Target to rebind " + target);
        actionToRebind.Disable();
        inputText.text = string.Format(Localization.GetString("uiStrings", "ui_input_rebind"), actionToRebind.name);
        rebindScreen.SetActive(true);
        var rebindOperation = actionToRebind.PerformInteractiveRebinding()
                    // To avoid accidental input from mouse motion
                    //.WithControlsExcluding("Mouse")
                    .WithTargetBinding(target)
                    .WithExpectedControlType(controlType)
                    .OnMatchWaitForAnother(0.1f)
                    .OnComplete(
                   operation =>
                   {
                       Debug.Log($"Rebound '{actionToRebind}' to '{operation.selectedControl}'");
                       operation.Dispose();
                       StopRebind(actionToRebind);
                   });
        /*if (excludeMouse)                   
            rebindOperation.WithControlsExcluding("Mouse");*/

        rebindOperation.Start();
    }

    public void StopRebind(InputAction actionToRebind)
    {
        actionToRebind.Enable();
        rebindScreen.SetActive(false);
        onRebindOver();
        SaveRebind();
    }


    /// <summary>
    /// stores the active control overrides to player prefs
    /// </summary>
    public void SaveRebind()
    {
        //saving
        BindingWrapperClass bindingList = new BindingWrapperClass();

        foreach (var map in playerInput)
        {
            foreach (var binding in map.bindings)
            {
                if (!string.IsNullOrEmpty(binding.overridePath))
                {
                    bindingList.bindingList.Add(new BindingSerializable(binding.id.ToString(), binding.overridePath));
                }
            }
        }

        PlayerPrefs.SetString("ControlOverrides", JsonUtility.ToJson(bindingList));
        PlayerPrefs.Save();
    }

    public void DeleteRebinds()
    {
        PlayerPrefs.DeleteKey("ControlOverrides");
        foreach (var map in playerInput)
        {
            map.RemoveAllBindingOverrides();
        }
    }

    /// <summary>
    /// Loads control overrides from playerprefs
    /// </summary>
    public void LoadRebinds()
    {
        if (PlayerPrefs.HasKey("ControlOverrides"))
        {
            BindingWrapperClass bindingList = JsonUtility.FromJson(PlayerPrefs.GetString("ControlOverrides"), typeof(BindingWrapperClass)) as BindingWrapperClass;

            //create a dictionary to easier check for existing overrides
            Dictionary<System.Guid, string> overrides = new Dictionary<System.Guid, string>();
            foreach (var item in bindingList.bindingList)
            {
                overrides.Add(new System.Guid(item.id), item.path);
            }

            //walk through action maps check dictionary for overrides
            foreach (var map in playerInput)
            {
                var bindings = map.bindings;
                for (var i = 0; i < bindings.Count; ++i)
                {
                    if (overrides.TryGetValue(bindings[i].id, out string overridePath))
                    {
                        //if there is an override apply it
                        map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        playerInput.Enable();
    }

    private void OnDisable()
    {
        if(playerInput!=null)
            playerInput.Disable();
    }
}
