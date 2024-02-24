using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NavController : MonoBehaviour
{
    public GameObject Display, Offline, Battery, map, MapCamera, MapTarget;
    public CircleRenderer[] circles;
    public Text batInd, scpInd;

    Equipable_Nav Nav;
    GameItem currNav;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void OnEnable()
    {
        currNav = GameController.ins.player.GetComponent<PlayerControl>().equipment[(int)bodyPart.Hand];
        Nav = ((Equipable_Nav)ItemController.instance.items[currNav.itemFileName]);

        if (GameController.ins.mapless)
        {
            Display.SetActive(false);
            return;
        }

        if (Nav.isOnline)
        {
            GameController.ins.Map_RenderFull();
            Offline.SetActive(false);
        }
        else
        {
            GameController.ins.Map_RenderHalf();
            Offline.SetActive(true);
        }

        if (Nav.SpendBattery)
        {
            Battery.SetActive(true);
            batInd.gameObject.SetActive(true);
        }
        else
        {
            Battery.SetActive(false);
            batInd.gameObject.SetActive(false);
        }

        if (Nav.isRadar)
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].gameObject.SetActive(true);
            }
            scpInd.gameObject.SetActive(true);
        }
        else
        {
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].gameObject.SetActive(false);
            }
            scpInd.gameObject.SetActive(false);
        }

        if (currNav.valFloat < 0 && Nav.SpendBattery)
            Display.SetActive(false);
        else
            Display.SetActive(true);
    }

    private void Update()
    {
        if (GameController.ins.mapless)
        {
            return;
        }

        if (Nav.SpendBattery)
        {
            int batPercent = ((int)Mathf.Floor((currNav.valFloat / (100 / 100)) / 5));

            if (currNav.valFloat <= 0)
                Display.SetActive(false);

            //batteryRect.sizeDelta = new Vector2(batPercent * 8, 14);
            string batEx = "[";
            for (int i = 0; i < 10; i++)
            {
                if (currNav.valFloat > (10 * i))
                    batEx += "█";
                else
                    batEx += "-";
            }
            batEx += "]";
            batInd.text = batEx;
        }

        MapCamera.transform.position = new Vector3((GameController.ins.player.transform.position.x / GameController.ins.roomsize) + 0.5f, (GameController.ins.player.transform.position.z / GameController.ins.roomsize) + 0.5f, MapCamera.transform.position.z);
        MapTarget.transform.position = new Vector3((GameController.ins.player.transform.position.x / GameController.ins.roomsize) + 0.5f, (GameController.ins.player.transform.position.z / GameController.ins.roomsize) + 0.5f, MapTarget.transform.position.z);
        MapTarget.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -GameController.ins.player.transform.eulerAngles.y);

        if (Nav.isRadar)
        {
            Vector2Int plyPos = GameController.ins.WorldToMap(GameController.ins.player.transform.position);

            int enmIdx = (int)npc.scp173;
            Vector2Int enmPos = (GameController.ins.npcController.mainList[enmIdx].data.isActive) ? GameController.ins.WorldToMap(GameController.ins.npcController.mainList[enmIdx].transform.position) : Vector2Int.one * int.MaxValue;

            string scpText = "";

            //float testRad = (Vector2.Distance(plyPos, enmPos));

            float rad = CalculateRad(plyPos, enmPos, circles[0].gameObject, ref scpText, "SCP-173\n");

            circles[0].transform.position = new Vector3((GameController.ins.player.transform.position.x / GameController.ins.roomsize) + 0.5f, (GameController.ins.player.transform.position.z / GameController.ins.roomsize) + 0.5f, circles[0].transform.position.z);
            circles[0].UpdateRadius(rad);

            enmIdx = (int)npc.scp106;
            enmPos = (GameController.ins.npcController.mainList[enmIdx].data.isActive) ? GameController.ins.WorldToMap(GameController.ins.npcController.mainList[enmIdx].transform.position) : Vector2Int.one * int.MaxValue;
            rad = CalculateRad(plyPos, enmPos, circles[1].gameObject, ref scpText, "SCP-106\n");
            circles[1].transform.position = new Vector3((GameController.ins.player.transform.position.x / GameController.ins.roomsize) + 0.5f, (GameController.ins.player.transform.position.z / GameController.ins.roomsize) + 0.5f, circles[0].transform.position.z);
            circles[1].UpdateRadius(rad);

            enmIdx = (int)npc.scp096;
            enmPos = (GameController.ins.npcController.mainList[enmIdx].data.isActive) ? GameController.ins.WorldToMap(GameController.ins.npcController.mainList[enmIdx].transform.position) : Vector2Int.one * int.MaxValue;

            rad = CalculateRad(plyPos, enmPos, circles[2].gameObject, ref scpText, "SCP-096\n");
            circles[2].transform.position = new Vector3((GameController.ins.player.transform.position.x / GameController.ins.roomsize) + 0.5f, (GameController.ins.player.transform.position.z / GameController.ins.roomsize) + 0.5f, circles[0].transform.position.z);
            circles[2].UpdateRadius(rad);

            enmIdx = (int)npc.scp049;
            enmPos = (GameController.ins.npcController.mainList[enmIdx].data.isActive) ? GameController.ins.WorldToMap(GameController.ins.npcController.mainList[enmIdx].transform.position) : Vector2Int.one * int.MaxValue;

            rad = CalculateRad(plyPos, enmPos, circles[3].gameObject, ref scpText, "SCP-049\n");
            circles[3].transform.position = new Vector3((GameController.ins.player.transform.position.x / GameController.ins.roomsize) + 0.5f, (GameController.ins.player.transform.position.z / GameController.ins.roomsize) + 0.5f, circles[0].transform.position.z);
            circles[3].UpdateRadius(rad);

            scpInd.text = scpText;
        }


    }

    float CalculateRad(Vector2Int playerPos, Vector2Int enemyPos, GameObject circle, ref string text, string textToAdd)
    {
        float rad = (Vector2.Distance(playerPos, enemyPos));
        rad = Mathf.Max(rad, 0.5f);

        if (rad > 3)
        {
            circle.SetActive(false);
        }
        else
        {
            text += textToAdd;
            circle.SetActive(true);
        }

        return rad;
    }




}
