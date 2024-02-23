using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoEndMenu : MonoBehaviour
{
    public AudioClip song;
    // Start is called before the first frame update
    void Start()
    {
        LoadingSystem.instance.FadeIn(2, new Vector3Int(0, 0, 0));
        MusicPlayer.instance.ChangeMusic(song);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Exit()
    {
        SceneManager.LoadScene(0);
    }
}
