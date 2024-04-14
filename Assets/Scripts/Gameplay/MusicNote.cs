using Melanchall.DryWetMidi.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicNote : MonoBehaviour
{
    public float Timing;
    public float LengthSeconds;
    public float LengthBeats;
    public int Lane;
    public Note FullNote;
    public bool IsLift;
    public LineRenderer HoldLine;

    public void Setup(int Lane, float Timing, float LengthSeconds, float LengthBeats)
    {
        this.Timing = Timing;
        this.LengthSeconds = LengthSeconds;
        this.LengthBeats = LengthBeats;
        this.Lane = Lane;
        transform.localPosition = new Vector3(this.Lane - 2, 0, Timing * NotesManager.Instance.ScrollSpeed);
        
        if (!this.IsLift && LengthBeats > 0.25)
            HoldLine.SetPositions(new Vector3[2]{ Vector3.zero, new Vector3(0, 0.001f, this.LengthSeconds * NotesManager.Instance.ScrollSpeed) });
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
