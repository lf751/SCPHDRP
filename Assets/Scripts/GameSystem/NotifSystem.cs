using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Pixelplacement;
using Pixelplacement.TweenSystem;

public class NotifSystem : MonoBehaviour
{
    public RectTransform myrect;
    public Image image;
    public Text Header;
    public Text body;
    // Start is called before the first frame update
    void Start()
    {
        Tween.AnchoredPosition(myrect, new Vector2(myrect.anchoredPosition.x,0), 1, 0, Tween.EaseOut, Tween.LoopType.None, null, () => Tween.AnchoredPosition(myrect, new Vector2(myrect.anchoredPosition.x, 142), 1, 6, Tween.EaseIn, Tween.LoopType.None, null, () => Destroy(this.gameObject), false), false);
    }
}
