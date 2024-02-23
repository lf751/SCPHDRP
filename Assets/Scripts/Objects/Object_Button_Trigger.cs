using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object_Button_Trigger : Object_Interact
{
    public AudioClip clicked;
    AudioSource audioSource;
    float deactivate;
    public bool activated;
    // Start is called before the first frame update
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        deactivate -= Time.deltaTime;
        if (deactivate <= 0)
            activated = false;
    }

    public override void Pressed()
    {
        audioSource.PlayOneShot(clicked);
        deactivate = 1.0f;
        activated = true;
        SubtitleEngine.instance.playSub("playStrings", "play_button");
    }

    public override void Hold()
    {
    }
}
