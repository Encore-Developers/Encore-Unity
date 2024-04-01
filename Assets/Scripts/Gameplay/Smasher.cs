using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

public class Smasher : MonoBehaviour
{
    public bool HeldLastFrame;
    public int Lane;
    MeshRenderer _mr;

    private void Awake()
    {
        _mr = GetComponentInChildren<MeshRenderer>();
    }
    
    public void Smash()
    {
        List<MusicNote> MyLane = GameManager.Instance.NotesByLane[Lane];
        MusicNote NextNote = MyLane[0];

        if (!NextNote)
            return;

        float Off = Mathf.Abs(NextNote.Timing - NotesManager.Instance.CurrentTime);
        if (Off > 1f)
            return;

        print($"Hit - Mathf.Abs(Off) = {Off}");

        if (Off <= 0.025f) // between -0.025 and 0.025
        {
            GameManager.Instance.PerfectHit();

            MyLane.Remove(NextNote);
            GameManager.Instance.SortedNotes.Remove(NextNote);
            Destroy(NextNote.gameObject);
            return;
        }

        if (Off > 0.025f && Off <= 0.125f) // between -0.125 and 0.125, skipping 0.025
        {
            GameManager.Instance.GoodHit();

            MyLane.Remove(NextNote);
            GameManager.Instance.SortedNotes.Remove(NextNote);
            Destroy(NextNote.gameObject);
            return;
        }
    }

    public void UpdateSmasherMaterial(float Tapped)
    {
        if (Tapped > 0)
            _mr.material = NotesManager.Instance.SmasherHit;
        else
            _mr.material = NotesManager.Instance.SmasherDefault;
    }
}
