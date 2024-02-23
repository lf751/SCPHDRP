using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RebindInput : MonoBehaviour
{
    public Text prim, sec, inputName, primBind, secBind;
    public gameplayActions bind;

    public bool defaultOnly=false;
    public bool hasAlt;
    // Start is called before the first frame update
    void Start()
    {
        if (!defaultOnly)
        RepaintText();
    }

    public void DefaultBindings()
    {
        SCPInput.instance.DeleteRebinds();
    }

    public void EndRebind()
    {
        RepaintText();
        SCPInput.instance.onRebindOver -= EndRebind;
    }
    public void DoRebind(bool isAlt)
    {
        SCPInput.instance.onRebindOver += EndRebind;
        SCPInput.instance.DoRebind(bind, isAlt);
        
    }

    void RepaintText()
    {
        prim.text = Localization.GetString("uiStrings", "ui_input_primary");
        if (hasAlt)
            sec.text = Localization.GetString("uiStrings", "ui_input_secondary");

        //Debug.Log("new input name = " + SCPInput.instance.GetBindName(bind, false));
        primBind.text = SCPInput.instance.GetBindName(bind, false);
        if (hasAlt)
            secBind.text = SCPInput.instance.GetBindName(bind, true);

        inputName.text = Localization.GetString("uiStrings", "ui_input_" + bind.ToString());

    }
}
