using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleStep : MonoBehaviour
{
    public AudioClip[] steps;
    AudioSource foot;
    // Start is called before the first frame update
    void Start()
    {
        foot = GetComponent<AudioSource>();
    }

    void StepSound()
    {
        foot.PlayOneShot(steps[Random.Range(0, steps.Length)]);
    }
}
