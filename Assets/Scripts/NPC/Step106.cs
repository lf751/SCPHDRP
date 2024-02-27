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
            DecalSystem.instance.Decal(ray.point, Quaternion.Euler(0f, Random.Range(-180f, 180f), 0f), Random.Range(1.8f, 2.3f), false, 0.6f, 2, 0);
        }

    }
}
