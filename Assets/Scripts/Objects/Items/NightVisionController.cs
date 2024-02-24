using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class NightVisionController : MonoBehaviour
{
    public Image[] bats;

    public static NightVisionController ins;

    Equipable_Elec_Loader NvDef;
    GameItem currNv;
    public Volume vol;
    bool closeTo895, efec895;
    public float distance;
    public RawImage jumpText;
    // Start is called before the first frame update
    void Start()
    {
        ins = this;
        currNv = GameController.ins.player.GetComponent<PlayerControl>().equipment[(int)bodyPart.Head];
        NvDef = ((Equipable_Elec_Loader)ItemController.instance.items[currNv.itemFileName]);
        closeTo895 = false;
        distance = Mathf.Infinity;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.ins.isAlive && GameController.ins.doGameplay)
        {
            //Debug.Log("bat" + currNv.valFloat);
            if (NvDef.SpendBattery)
            {
                if (currNv.valFloat > 0)
                {
                    int batPercent = (Mathf.CeilToInt((10 * currNv.valFloat) / 100));

                    for (int i = 0; i < 10; i++)
                    {
                        if (batPercent > 0)
                            bats[i].fillCenter = true;
                        else
                            bats[i].fillCenter = false;
                        batPercent -= 1;
                    }
                }
                else
                {
                    GameController.ins.currPly.BlinkingTimer = 0;
                    GameController.ins.currPly.CloseTimer = GameController.ins.currPly.ClosedEyes;
                }
            }

            if (Time.frameCount % 15 == 0)
            {
                Vector2Int roomPos = GameController.ins.GetRoom("Heavy_COFFIN");
                if (roomPos.x == -1)
                {
                    closeTo895 = false;
                }
                else
                {
                    Vector2Int playerPos = new Vector2Int(GameController.ins.xPlayer, GameController.ins.yPlayer);
                    distance = Vector2.Distance(playerPos, roomPos);
                    //Debug.Log("Distance: " + distance);
                    if (distance < 2f)
                    {
                        closeTo895 = true;
                    }
                    else
                    {
                        closeTo895 = false;
                    }
                }
            }
        }
    }

}
