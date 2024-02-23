using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AmbianceController : MonoBehaviour
{
    public bool enableAmbiance;
    public AudioClip[] AmbianceLibrary;
    public AudioClip[] GenericAmbiance;
    float ambiancetimer = 0, GENambiancetimer = 0, ambiancefreq = 3;
    public float ambifreq, GENambiancefreq;
    public bool custom = true;
    public string eventBundleName;
    //private AssetBundle eventBundle;

    // Start is called before the first frame update

    // Update is called once per frame
    /*private void Start()
    {
        eventBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, eventBundleName));
    }*/

    void Update()
    {
        if (enableAmbiance && AmbianceLibrary.Length != 0)
        DoAmbiance();

    }

    void DoAmbiance()
    {
        ambiancetimer -= Time.deltaTime;
        if (ambiancetimer <= 0)
        {
            int i = Random.Range(0, AmbianceLibrary.Length);
            GameController.instance.MixAmbiance.PlayOneShot(AmbianceLibrary[i]);
            ambiancetimer = ambiancefreq * Random.Range(1, 5);
        }
    }

    public void GenAmbiance()
    {
        GENambiancetimer -= Time.deltaTime;
        if (GENambiancetimer <= 0)
        {
            int i = Random.Range(0, GenericAmbiance.Length);

            GameController.instance.MixAmbiance.PlayOneShot(GenericAmbiance[i]);
            GENambiancetimer = GENambiancefreq * Random.Range(2, 5);
        }
    }

    public void ChangeAmbiance(AudioClip[] NewAmbiance, float freq)
    {
        AmbianceLibrary = NewAmbiance;
        ambiancefreq = freq;
        custom = true;
        //GameController.instance.zoneAmbiance = -1;
    }

    public void NormalAmbiance(AudioClip[] NewAmbiance)
    {
        AmbianceLibrary = NewAmbiance;
        ambiancefreq = ambifreq;
        custom = false;
        //GameController.instance.zoneAmbiance = -1;
    }

    /*public AudioClip[] getEventAudioArray(string[] clips)
    {
        AudioClip[] returnedAudio = new AudioClip[clips.Length];
        
        for(int i = 0; i < clips.Length; i++)
        {
            returnedAudio[i] = eventBundle.LoadAsset<AudioClip>(clips[i]);
        }

        return returnedAudio;
    }

    public AudioClip getEventAudio(string clip)
    {
        return eventBundle.LoadAsset<AudioClip>(clip);
    }*/


}
