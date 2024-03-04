using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerMouseLook : MonoBehaviour
{
    public Vector3 rotation = new Vector3(0, 0, 0);
    public Quaternion addedRota = Quaternion.identity;
    public float speed = 3;
    float inspeed=3;
    bool Invert = false;
    public bool Instant;
    public bool inputActive = true;

    void LateUpdate()
    {
        float z = transform.eulerAngles.z;


        if (addedRota != Quaternion.identity)
        {
            rotation = addedRota.eulerAngles;
            rotation.x = (rotation.x > 180) ? rotation.x - 360 : rotation.x;
        }

        rotation.y = (rotation.y > 180) ? rotation.y - 360 : rotation.y;

        //Debug.Log("Mouse Vector = " + SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>());
        if (inputActive == true)
        {
            rotation.y += (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().x * inspeed) * Time.timeScale;
            if (Invert)
                rotation.x += (SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().y * inspeed) * Time.timeScale;
            else
                rotation.x += -(SCPInput.instance.playerInput.Gameplay.Look.ReadValue<Vector2>().y * inspeed) * Time.timeScale;
        }
        rotation.x = Mathf.Clamp(rotation.x, -85f, 75f);



        rotation.z = z;



        transform.rotation =  Quaternion.Euler(rotation);
    }
    public void LoadValues()
    {
        inspeed = speed * PlayerPrefs.GetFloat("MouseAcc", 1);
        Invert = (PlayerPrefs.GetInt("Invert", 0) == 1);
    }

}