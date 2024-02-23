using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeController : MonoBehaviour
{
    public bool isActive;
    bool smokeActive = true;
    public ParticleSystem smokeEffect;
    public CapsuleCollider smokeCollider;
    public AudioSource smokeSound;
    float usualRadius;

    // Update is called once per frame
    void Start()
    {
        usualRadius = smokeCollider.radius;
        Switch(isActive);
    }

    public void Switch(bool State)
    {
        isActive = State;

        if (isActive != smokeActive)
        {
            if (smokeEffect != null)
            {
                var emission = smokeEffect.emission;
                emission.enabled = isActive;
            }

            smokeCollider.radius = (isActive ? usualRadius : 0);
            smokeSound.mute = !isActive;

            smokeActive = isActive;
        }

    }
}
