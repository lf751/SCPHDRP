using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessHallway : MonoBehaviour
{
    public Transform Pos1, Pos2;
    public BoxTrigger Box1, Box2;

    // Update is called once per frame
    void Update()
    {
        if (Box1.GetState())
        {
            if (Random.Range(0,100) < 40)
                PD_Teleports.instance.Teleport();
            else
                Switch(Pos1, Pos2);
        }

        if (Box2.GetState())
        {
            if (Random.Range(0, 100) < 40)
                PD_Teleports.instance.Teleport();
            else
                Switch(Pos2, Pos1);
        }
    }


    void Switch(Transform start, Transform end)
    {
        GameObject objPlayer = GameController.ins.player;
        objPlayer.GetComponent<PlayerControl>().playerWarp((end.transform.position + ((end.transform.rotation * Quaternion.Inverse(start.transform.rotation)) * (objPlayer.transform.position - start.position))), end.transform.eulerAngles.y - start.transform.eulerAngles.y);
        Debug.Log("Diferencia de Rotacion: " + (end.transform.eulerAngles.y - start.transform.eulerAngles.y));
    }
}
