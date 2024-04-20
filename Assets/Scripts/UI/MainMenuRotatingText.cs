using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class MainMenuRotatingText : MonoBehaviour
{
    public TextAsset Lines;
    List<string> TextRotation = new List<string>();
    TextMeshProUGUI _txt;

    private void Awake()
    {
        _txt = GetComponent<TextMeshProUGUI>();
        TextRotation = Lines.text.Replace("\r\n", "\n").Split("\n").ToList();
        StartCoroutine(TextRotationCoroutine());
    }

    IEnumerator TextRotationCoroutine()
    {
        while (true)
        {
            _txt.DOText(TextRotation[Random.Range(0, TextRotation.Count)], 0.6f, true);
            yield return new WaitForSeconds(5);
        }
    }
}
