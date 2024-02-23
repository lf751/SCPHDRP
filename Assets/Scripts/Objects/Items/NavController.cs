using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class NavController : MonoBehaviour
{
    public GameObject Display, Offline, Battery, map, MapCamera, MapTarget;
    public RectTransform batteryRect;
    
    Equipable_Nav Nav;
    gameItem currNav;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnEnable()
    {
        currNav = GameController.instance.player.GetComponent<Player_Control>().equipment[(int)bodyPart.Hand];
        Nav = ((Equipable_Nav)ItemController.instance.items[currNav.itemFileName]);
        if (Nav.isOnline)
        {
            GameController.instance.Map_RenderFull();
            Offline.SetActive(false);
        }
        else
        {
            GameController.instance.Map_RenderHalf();
            Offline.SetActive(true);
        }

        if (Nav.SpendBattery)
        {
            Battery.SetActive(true);
        }
        else
        {
            Battery.SetActive(false);
        }

        if (currNav.valFloat < 0 && Nav.SpendBattery)
            Display.SetActive(false);
        else
            Display.SetActive(true);
    }

    private void Update()
    {
        if (Nav.SpendBattery)
        {
            int batPercent = ((int)Mathf.Floor((currNav.valFloat / (100 / 100)) / 5));

            if (currNav.valFloat <= 0)
                Display.SetActive(false);

            batteryRect.sizeDelta = new Vector2(batPercent * 8, 14);
        }

        MapCamera.transform.position = new Vector3(GameController.instance.xPlayer, GameController.instance.yPlayer, MapCamera.transform.position.z);
        MapTarget.transform.position = new Vector3(GameController.instance.xPlayer + 0.5f, GameController.instance.yPlayer + 0.5f, MapTarget.transform.position.z);
        MapTarget.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -GameController.instance.player.transform.eulerAngles.y);
    }




}
