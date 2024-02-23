using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Step106 : MonoBehaviour
{
    public AudioClip[] steps;
    AudioSource foot;
    RaycastHit ray;
    public LayerMask Collision;
    // Start is called before the first frame update
    void Start()
    {
        foot = GetComponent<AudioSource>();
    }
    void StepSound()
    {
        
        foot.clip = steps[Random.Range(0, steps.Length)];
        foot.Play();
        if (Physics.Raycast(transform.position + (Vector3.up), Vector3.down, out ray, 1.5f, Collision, QueryTriggerInteraction.Ignore))
        {
            DecalSystem.instance.Decal(ray.point + (Vector3.up * 0.05f), new Vector3(90f, 0, 0), 2f, false, 0.4f, 2, 0);
        }

    }
}
