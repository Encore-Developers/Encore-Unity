using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smasher : MonoBehaviour
{
    public bool TappedThisFrame;
    public bool ReleasedThisFrame;
    public bool IsHeld;

    MeshRenderer _mr;

    private void Awake()
    {
        _mr = GetComponentInChildren<MeshRenderer>();
    }

    private void Update()
    {
        if (IsHeld)
            _mr.material = NotesManager.Instance.SmasherHit;
        else
            _mr.material = NotesManager.Instance.SmasherDefault;
    }
}
