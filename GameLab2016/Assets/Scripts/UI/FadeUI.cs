using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FadeUI : MonoBehaviour {

    public float fadeTime = 1f;

    [SerializeField]
    private Image fadeRect;

    public void StartFadeAnim(bool toBlack) {
        fadeRect.DOFade(toBlack ? 1 : 0, fadeTime);
    }
}
