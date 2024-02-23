using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Pixelplacement;
using Pixelplacement.TweenSystem;

[System.Serializable]
public class LoadingScreen
{
    public string name;
    public bool hasInfo2, hasInfo3;
    public string imagefile;
    public TextAnchor position;
}

public class LoadingSystem : MonoBehaviour
{

    public LoadingScreen[] screens;
    int screenshots=0;
    Vector3Int fadecolor;
    int loading = 0;
    public AudioClip done;
    public Canvas Loading;
    public Canvas SimpleLoading;
    public VerticalLayoutGroup layout;
    public Text title;
    public Text body;
    public Text start;
    public Image image;
    public Image fade;
    public Image loadcircle;
    public Image Simplecircle;
    public static LoadingSystem instance;
    public bool isFading;
    bool isLoading;
    public bool isLoadingDone;
    public float loadbar;
    private float _progress = 0f;
    bool Simple;

    bool _isClicked;
    public bool canClick;
    // Start is called before the first frame update

    private void Awake()
    {


        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    void DoHeavyLoading()
    {
        loading = Random.Range(0, screens.Length);

        isLoading = true;
        start.enabled = false;
        start.text = Localization.GetString("uiStrings", "ui_in_anykey");
        loadbar = 0;
        isLoadingDone = false;
        Loading.enabled = true;
        image.sprite = Resources.Load<Sprite>(string.Concat("LoadingManager/", screens[loading].imagefile));
        image.SetNativeSize();
        layout.childAlignment = screens[loading].position;
        title.text = Localization.GetString("loadStrings", string.Concat("title_", screens[loading].name));
        body.text = Localization.GetString("loadStrings", string.Concat("body1_", screens[loading].name));
    }

    // Update is called once per frame
    void Update()
    {
        /*if(Input.GetKeyDown(KeyCode.F7))
        {
            DoScreenShot();
        }*/
        if (isLoading)
        {
            if (loadbar > 0.3f && loadbar < 0.6f && screens[loading].hasInfo2)
                body.text = Localization.GetString("loadStrings", string.Concat("body2_", screens[loading].name));
            if (loadbar > 0.6f && screens[loading].hasInfo3)
                body.text = Localization.GetString("loadStrings", string.Concat("body3_", screens[loading].name));
        }

        loadcircle.fillAmount = loadbar;
        Simplecircle.fillAmount = loadbar; 

        if (SCPInput.instance.playerInput.Gameplay.Pause.triggered  && canClick)
        {
            _isClicked = true;
        }
    }

    public void FadeIn(float duration, Vector3Int newcolors)
    {
        fadecolor = newcolors;
        Tween.Value(1f, 0f, FadeUpdate, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, null, false);
    }
    public void FadeOut(float duration, Vector3Int newcolors)
    {
        fadecolor = newcolors;
        Tween.Value(0f, 1f, FadeUpdate, duration, 0, Tween.EaseInOut, Tween.LoopType.None, null, null, false);
    }

    void FadeUpdate(float value)
    {
        fade.color = new Color(fadecolor.x, fadecolor.y, fadecolor.z, value);
    }


    public void LoadLevel(int sceneIndex, bool _simple = true)
    {
        //SCPInput.instance.ToUI();

        Simple = _simple;
        if (!_simple)
            DoHeavyLoading();
        else
            SimpleLoading.enabled = true;

        _progress = 0f;
        StartCoroutine(LoadAsync(sceneIndex));
    }

    IEnumerator LoadAsync(int sceneIndex)
    {
        isLoadingDone = false;
        isLoading = true;
        canClick = false;
        _isClicked = false;
        _progress = 0;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        while (_progress < 1f)
        {
            _progress = Mathf.Clamp01(operation.progress / 0.9f);

            loadbar = _progress;

            yield return null;
        }

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }

        canClick = true;

        if (Simple)
            _isClicked = true;
        else
        {
            start.enabled = true;
            MusicPlayer.instance.Music.PlayOneShot(done);
        }


        while (!_isClicked)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));

        isLoadingDone = true;
        isLoading = false;

        if (Simple)
            SimpleLoading.enabled = false;
        else
            Loading.enabled = false;

    }


    public void LoadLevelHalf(int sceneIndex, bool dofade = false, float duration = -1, int r = 0, int g = 0, int b = 0, bool _Simple = false)
    {
        //SCPInput.instance.ToUI();
        Simple = _Simple;

        if (!_Simple)
            DoHeavyLoading();
        else
            SimpleLoading.enabled = true;
        _progress = 0f;
        StartCoroutine(LoadAsyncHalf(sceneIndex, dofade, new Vector3Int(r,g,b), duration));
    }

    IEnumerator LoadAsyncHalf(int sceneIndex, bool dofade, Vector3Int newcolor, float duration)
    {
        isLoading = true;
        isLoadingDone = false;
        canClick = false;
        _isClicked = false;

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        while (_progress < 1f)
        {
            _progress = Mathf.Clamp01(operation.progress / 0.9f);

            loadbar = (_progress/2);

            yield return null;
        }
        

        operation.allowSceneActivation = true;

        while (!canClick)
        {
            yield return null;
        }


        if (Simple)
            _isClicked = true;
        else
        {
            start.enabled = true;
            MusicPlayer.instance.Music.PlayOneShot(done);
        }


        while (!_isClicked)
        {
            yield return null;
        }

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(sceneIndex));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isLoadingDone = true;
        isLoading = false;

        if (Simple)
            SimpleLoading.enabled = false;
        else
            Loading.enabled = false;


        if (dofade)
        {
            FadeIn(duration, newcolor);
        }
    }


    void DoScreenShot()
    {
        Debug.Log("saycheese" + screenshots);
        ScreenCapture.CaptureScreenshot("C:/Users/Daniel/Documents/STEAMSCREENS/screen" + screenshots + ".png", 2);
        screenshots += 1;
    }

}
