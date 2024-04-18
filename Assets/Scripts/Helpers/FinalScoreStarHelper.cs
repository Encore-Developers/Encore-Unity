using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FinalScoreStarHelper : MonoBehaviour
{
    public enum StarState
    {
        NotEnabled,
        Basic,
        Golden
    }

    Image _img;
    public StarState CurState = StarState.NotEnabled;

    private void Start()
    {
        _img = GetComponent<Image>();
        _img.color = new Color(0.33f, 0.33f, 0.33f);
    }

    public IEnumerator Appear()
    {
        CurState = StarState.Basic;
        _img.DOColor(Color.white, 0.6f);
        yield break;
    }

    public IEnumerator Gold()
    {
        CurState = StarState.Golden;
        _img.DOColor(Color.yellow, 0.6f);
        yield break;
    }
}
