using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{

    public static MusicPlayer instance = null;
    public AudioSource Music;
    bool changeTrack, changed;
    AudioClip trackTo;

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

    // Update is called once per frame
    void Update()
    {
        if (changeTrack == true)
            MusicChanging();
    }

    public void ChangeMusic(AudioClip newMusic)
    {
        changeTrack = true;
        trackTo = newMusic;
        changed = false;
    }

    public void StartMusic(AudioClip newMusic)
    {
        Music.Stop();
        Music.volume = 1f;
        Music.clip = newMusic;
        Music.Play();
    }

    public void StopMusic()
    {
        changeTrack = true;
        trackTo = null;
        changed = false;
    }

    void MusicChanging()
    {
        if (changed == false)
            Music.volume -= (Time.deltaTime) / 4;

        if (Music.volume <= 0.1 && changed == false && trackTo != null)
        {
            changed = true;
            Music.clip = trackTo;
            Music.Play();
        }

        if (changed == true)
            Music.volume += Time.deltaTime;

        if (Music.volume >= 0.9 && changed == true)
        {
            changeTrack = false;
        }


    }

}
