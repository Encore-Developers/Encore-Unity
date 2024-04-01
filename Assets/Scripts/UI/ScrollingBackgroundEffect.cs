using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackgroundEffect : MonoBehaviour
{
    Material _mat;
    Image _img;

    private void Start()
    {
        _img = GetComponent<Image>();
        _mat = Instantiate(_img.material);

        if (transform is RectTransform rt)
        {
            _mat.mainTextureScale = new Vector2(rt.sizeDelta.x / rt.sizeDelta.y, 100 / rt.sizeDelta.y);
            _img.material = _mat;
        }
    }

    private void Update()
    {
        _img.materialForRendering.mainTextureOffset = new Vector2(-Time.time * 0.05f, Time.time * 0.05f);
        _img.SetMaterialDirty();
        //transform.position += new Vector3(0.00001f, 0);
        //transform.position -= new Vector3(0.00001f, 0);
    }
}
