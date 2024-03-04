using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{

    public static MusicPlayer instance = null;
    public AudioSource Music;
    bool changeTrack, changed;
    AudioClip trackTo;
    float maxVolume = 0.5f;

    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
        Music.ignoreListenerPause = true;


    }

    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (changeTrack == true)
            MusicChanging();
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        if (newMusic == trackTo) return;
        changeTrack = true;
        Music.clip = trackTo;
        trackTo = newMusic;
        changed = false;
        Debug.Log("Changing playing " + newMusic.name);
    }

    public void StartMusic(AudioClip newMusic)
    {
        Music.Stop();
        Music.volume = maxVolume;
        Music.clip = newMusic;
        Music.Play();
        Debug.Log("Starting playing " + newMusic.name);
    }

    public void StopMusic()
    {
        changeTrack = true;
        trackTo = null;
        changed = false;
        Debug.Log("Stoping music");
    }

    void MusicChanging()
    {
        if (changed == false)
            Music.volume -= (Time.deltaTime) / 4;

        if (Music.volume <= 0.01 && changed == false && trackTo != null)
        {
            changed = true;
            Music.clip = trackTo;
            if (trackTo != null)
                Music.Play();
        }

        if (changed == true)
            Music.volume += Time.deltaTime;

        if (Music.volume >= maxVolume && changed == true)
        {
            changeTrack = false;
        }


    }

}
