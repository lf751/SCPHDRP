using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class delay_data
{
    public subtitleMeta sub;
    public float time;

    public delay_data (subtitleMeta _sub, float _time)
    {
        sub = _sub;
        time = _time;
    }
}

class time_data
{
    public string sub;
    public float time;

    public time_data(string _sub, float _time)
    {
        sub = _sub;
        time = _time;
    }
}

public class SubtitleEngine : MonoBehaviour
{
    public static SubtitleEngine instance = null;
    const string stage_direction = "<b>{0}</b>: {1}";
    float subtitle_add;
    float[] subtitle_hold = new float [3];
    float[] flavortext_hold = new float[3];
    public float subtitle_hold_timer, subtitle_add_timer;
    bool foundSub, foundFlavortext;
    bool Moved, MoverFlavorText;
    bool VoiceSubsEnabled = true;

    Color boxcol;

    [Header ("Subtitles")]
    public Text [] line = new Text [3];
    public Image[] bg = new Image[3];
    public RectTransform[] bgsize = new RectTransform[3];
    List<time_data> pending = new();
    List<delay_data> delayed = new();
    string [] current = new string [3];

    [Header("FlavorText")]
    public Text[] ft_line = new Text[3];
    public Image[] ft_bg = new Image[3];
    public RectTransform[] ft_bgsize = new RectTransform[3];
    List<string> ft_pending = new();
    string[] ft_current = new string[3];

    // Start is called before the first frame update

    public void LoadValues()
    {
        VoiceSubsEnabled = (PlayerPrefs.GetInt("Sub", 1) == 1);
    }


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != null)
            Destroy(gameObject);
        boxcol = bg[0].color;
    }

    public void playSub(string table, string id)
    {
        string sub = Localization.GetString(table, id);
            flavortext_hold[2] = 0;
            ft_pending.Add(sub);

    }

    public void playFormatted(string table, string id, string table2, string id2)
    {
        string format = Localization.GetString(table, id);
        string sub = Localization.GetString(table2, id2);
        flavortext_hold[2] = 0;
        ft_pending.Add(string.Format(format, sub));
    }

    public void playVoice(string id, bool Force = false)
    {
        Debug.Log("Buscando Subtitulo " + id);
        if (VoiceSubsEnabled)
        {
            if (Force)
            {
                pending.Clear();
                delayed.Clear();
                subtitle_hold[2] = 0;
                Debug.Log(" FORCE SUB ");
            }
            subtitleMeta sub = Localization.GetSubtitle(id);

            doSub(sub);
            
        }

    }

    public void doSub(subtitleMeta sub)
    {
        

        string buildSubtitle;
        if (!sub.noFormat)
            buildSubtitle = string.Format(stage_direction, Localization.GetString("charaStrings", sub.character), sub.subtitle);
        else
            buildSubtitle = sub.subtitle;

        //Debug.Log("Subtitulo " + buildSubtitle + " con el extra " + sub.nextSubtitle);

        if (!string.IsNullOrEmpty(sub.nextSubtitle))
            playVoice(sub.nextSubtitle);

        sub.nextSubtitle = "";

        if (sub.delay > 0)
            delayed.Add(new delay_data(sub, sub.delay));
        else
        {
            if (buildSubtitle.Length > 60)
            {
                int firstspace = buildSubtitle.IndexOf(" ", 60);
                if (firstspace != -1)
                {
                    subtitleMeta midSub = sub;
                    sub.duration = (sub.duration / 2) < 2 ? 2 : (sub.duration / 2);
                    midSub.duration = sub.duration;
                    midSub.noFormat = true;
                    midSub.subtitle = buildSubtitle[(firstspace + 1)..];

                    if (!string.IsNullOrEmpty(midSub.subtitle))
                        delayed.Add(new delay_data(midSub, sub.duration));

                    buildSubtitle = buildSubtitle[..firstspace];
                }
            }
            pending.Add(new time_data(buildSubtitle, sub.duration));
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        //subtitle_add -= Time.deltaTime;

        if (delayed.Count > 0)
        {
            doDelayed();
        }

        /*if (subtitle_add <= 0)
        {*/
            if (pending.Count > 0)
            {
                moveSubtitles();
                addSubtitles();
                //subtitle_add = subtitle_add_timer;
            }
        //}

        updateSubtitles();

        for (int i = 0; i < 3; i++)
        {
            line[i].text = current[i];
            if (string.IsNullOrEmpty(current[i]))
                bg[i].color = Color.clear;
            else
            {
                bg[i].color = boxcol;
                bgsize[i].sizeDelta = new Vector2(line[i].preferredWidth+20, bgsize[i].sizeDelta.y);
            }
        }


        if (ft_pending.Count > 0)
        {
            moveFlavorText();
            addFlavorText();
        }

        if (foundFlavortext)
            updateFlavorText();

        for (int i = 0; i< 3; i++)
        {
            ft_line[i].text = ft_current[i];
            if (string.IsNullOrEmpty(ft_current[i]))
                ft_bg[i].color = Color.clear;
            else
            {
                ft_bg[i].color = boxcol;
                ft_bgsize[i].sizeDelta = new Vector2(ft_line[i].preferredWidth+20, ft_bgsize[i].sizeDelta.y);
}
        }

    }

    void doDelayed()
    {
        for (int i = 0; i < delayed.Count; i++)
        {
            delayed[i].time -= Time.deltaTime;
            if (delayed[i].time < 0)
            {
                subtitleMeta sub = delayed[i].sub;
                sub.delay = 0;
                doSub(sub);
                
                delayed.RemoveAt(i);
            }
        }
    }

    void moveSubtitles()
    {
        for (int i = 1; i >= 0; i--)
        {
            if (current[i+1]==null)
            {
                current[i + 1] = current[i];
                current[i] = null;
                subtitle_hold[i + 1] = subtitle_hold[i];
            }
        }
    }

    void addSubtitles()
    {
            if (current[0] == null)
            {
                current[0] = pending[0].sub;
                subtitle_hold[0] = pending[0].time;
                pending.RemoveAt(0);
                foundSub = true;
                return;
            }
    }

    void updateSubtitles()
    {
        foundSub = false;
        for (int i = 0; i < 3; i++)
        {
            if (subtitle_hold[i] >= 0)
            {
                subtitle_hold[i] -= Time.deltaTime;
                foundSub = true;
            }
            else
            {
                current[i] = null;
            }
        }
    }

    void moveFlavorText()
    {
        for (int i = 1; i >= 0; i--)
        {
            if (ft_current[i + 1] == null)
            {
                ft_current[i + 1] = ft_current[i];
                ft_current[i] = null;
                flavortext_hold[i + 1] = flavortext_hold[i];
            }
        }
    }

    void addFlavorText()
    {
        if (ft_current[0] == null)
        {
            ft_current[0] = ft_pending[0];
            flavortext_hold[0] = subtitle_hold_timer;
            ft_pending.RemoveAt(0);
            foundFlavortext = true;
            return;
        }
    }

    void updateFlavorText()
    {
        foundFlavortext = false;
        for (int i = 0; i < 3; i++)
        {
            if (flavortext_hold[i] >= 0)
            {
                flavortext_hold[i] -= Time.deltaTime;
                foundFlavortext = true;
            }
            else
            {
                ft_current[i] = null;
            }
        }
    }


}
