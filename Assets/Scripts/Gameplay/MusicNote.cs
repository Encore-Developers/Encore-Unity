using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicNote : MonoBehaviour
{
    public float Timing;
    public int Lane;
    public Note FullNote;

    public void Setup(int Lane, float Timing)
    {
        this.Timing = Timing;
        this.Lane = Lane;

        transform.localPosition = new Vector3(this.Lane - 2, 0, Timing * NotesManager.Instance.ScrollSpeed);
    }

    /*private void Update()
    {
        if (transform.position.z < 0)
        {
            GameManager.Instance.NotesByLane[Lane].Remove(this);
            Destroy(gameObject);
        }
    }*/
}
