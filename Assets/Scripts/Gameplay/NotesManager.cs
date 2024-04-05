using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;

public enum StemType
{
    Backing,
    Vocals,
    Lead,
    Bass,
    Drums
}

public enum InstrumentType
{
    VOCALS,
    GUITAR,
    BASS,
    DRUMS,
    PLASTICGUITAR,
    PLASTICBASS,
    PLASTICDRUMS
}

public class NotesManager : MonoSingleton<NotesManager>
{
    public override bool DontDestroy() => false;

    public Transform NotesParent;
    public float MusicSyncDeltaTime = 0;
    public Material SmasherHit, SmasherDefault;
    public float ScrollSpeed = 15f;
    public Dictionary<StemType, AudioSource> LoadedStems = new Dictionary<StemType, AudioSource>();

    public float CurrentTime
    {
        get => _backingSource?.time ?? -1;
    }

    AudioSource _backingSource = null;
    Dictionary<SongDifficulty, List<KeyValuePair<int, int>>> _difficultyNoteBounds = new Dictionary<SongDifficulty, List<KeyValuePair<int, int>>>()
    {
        // Difficulty, { LowerBound, HigherBound }
        { SongDifficulty.Expert, new List<KeyValuePair<int, int>>{new KeyValuePair<int,int>(96, 100), new KeyValuePair<int,int>(102,106) }},
        { SongDifficulty.Hard,   new List<KeyValuePair<int, int>>{new KeyValuePair<int,int>(84, 87), new KeyValuePair<int,int>(90, 93)} },
        { SongDifficulty.Medium, new List<KeyValuePair<int, int>>{new KeyValuePair<int,int>(72, 75), new KeyValuePair<int,int>(78,81)} },
        { SongDifficulty.Easy,   new List<KeyValuePair<int, int>>{new KeyValuePair<int,int>(60, 63), new KeyValuePair<int,int>(66,69)} }
    };

    public void LoadStems(SongConfigTemplate Config)
    {
        foreach (FieldInfo F in typeof(SongStemsConfigurationTemplate).GetFields())
        {
            StemType StemT = Enum.Parse<StemType>(F.Name);
            string FileName = (string)F.GetValue(Config.StemFileNames);
            if (FileName == null)
                continue;

            print($"Got {FileName} for stem type {StemT}");
            AudioClip Clip = OggVorbis.VorbisPlugin.Load(Path.Combine(Config.SongDirectory, FileName));

            AudioSource StemSource = Camera.main.gameObject.AddComponent<AudioSource>();
            StemSource.clip = Clip;
            StemSource.volume = 1f; // TODO: settings where you can adjust the volume
            if (StemT == StemType.Backing)
                _backingSource = StemSource; // backing is gonna be the master, rest will be slaves

            LoadedStems.Add(StemT, StemSource);
        }

        if (_backingSource == null)
            throw new Exception("Loaded track has no backing track source - cannot continue without a master for our slaves!");

        print("Stems loaded!");
    }

    public void LoadNotes(SongConfigTemplate Config, InstrumentType Instrument, SongDifficulty Difficulty)
    {
        TempoMap Tempos = Config.Chart.GetTempoMap();
        Dictionary<string, TrackChunk> PlayableTracks = Config.Chart.GetTrackChunks()
            .ToDictionary(K => (K.Events.First(y => y.EventType == MidiEventType.SequenceTrackName) as SequenceTrackNameEvent).Text.Replace("PART ", "").Replace(" ", ""), V => V);

        TrackChunk InstChunk = PlayableTracks.First(x => x.Key == Instrument.ToString()).Value;
        List<KeyValuePair<int, int>> DiffBounds = _difficultyNoteBounds[Difficulty];
        //print(string.Join(", ", PlayableTracks.Keys));

        List<Note> TrackNotes = InstChunk
            .GetNotes()
            .Where(x => x.NoteNumber >= DiffBounds[0].Key && x.NoteNumber <= DiffBounds[0].Value)
            .ToList();
        List<Note> TrackLifts = InstChunk
            .GetNotes()
            .Where(x => x.NoteNumber >= DiffBounds[1].Key && x.NoteNumber <= DiffBounds[1].Value)
            .ToList();
        GameObject NoteBase = Resources.Load<GameObject>("Prefabs/Note");
        GameObject LiftBase = Resources.Load<GameObject>("Prefabs/Lift");
        //for (int i = 0; i < 5; i++)
        //{
            //MusicNote MN = Instantiate(NoteBase, NotesParent).GetComponent<MusicNote>();
            //MN.Setup(i, i * 0.5f + 5);
        //}

        List<MusicNote> SortedNotes = new List<MusicNote>();
        List<List<MusicNote>> Lanes = new List<List<MusicNote>>()
        {
            new List<MusicNote>(), // Leftmost Lane
            new List<MusicNote>(),
            new List<MusicNote>(),
            new List<MusicNote>(),
            new List<MusicNote>()  // Rightmost Lane
        };
        
        int LiftIdx = 0;
        int NoteIdx = 0;
        foreach (Note Note in TrackNotes)
        {
            bool IsLift = false;
            if(LiftIdx<TrackLifts.Count){
                if((float)Note.TimeAs<MetricTimeSpan>(Tempos).TotalSeconds == (float)TrackLifts[LiftIdx].TimeAs<MetricTimeSpan>(Tempos).TotalSeconds){
                    IsLift=true;
                    LiftIdx++;
                }
            }
            
            MusicNote MN = Instantiate(IsLift ? LiftBase : NoteBase, NotesParent).GetComponent<MusicNote>();
            MN.IsLift = IsLift;
            MN.FullNote = Note;
            Debug.Log($"Note {NoteIdx} length is {(float)Note.TimeAs<MetricTimeSpan>(Tempos).TotalSeconds}, length in beats is {(float)(Note.LengthAs<BarBeatFractionTimeSpan>(Tempos).Bars+Note.LengthAs<BarBeatFractionTimeSpan>(Tempos).Beats)}");
            MN.Setup(Note.NoteNumber - DiffBounds[0].Key, (float)Note.TimeAs<MetricTimeSpan>(Tempos).TotalSeconds, (float)Note.LengthAs<MetricTimeSpan>(Tempos).TotalSeconds, (float)(Note.LengthAs<BarBeatFractionTimeSpan>(Tempos).Bars+Note.LengthAs<BarBeatFractionTimeSpan>(Tempos).Beats));

            Lanes[MN.Lane].Add(MN);
            SortedNotes.Add(MN);
            //print(MN.Timing);
        }

        GameManager.Instance.SortedNotes = SortedNotes.OrderBy(x => x.Timing).ToList(); // just in case
        GameManager.Instance.NotesByLane = Lanes;

        GameManager.Instance.CurrentGameStats.TotalNotes = SortedNotes.Count;
    }

    private void Update()
    {
        if (_backingSource == null)
            return;

        NotesParent.position = new Vector3(0, 0, -_backingSource.time * ScrollSpeed + 2.35f); // the -2.35f is an offset

        for (int i = 0; i < GameManager.Instance.SortedNotes.Count; i++)
        {
            MusicNote N = GameManager.Instance.SortedNotes[i];
            if (N.transform.position.z < 0)
            {
                GameManager.Instance.CurrentGameStats.Misses++;
                GameManager.Instance.CurrentGameStats.CurrentCombo = 0;
                GameManager.Instance.ComboText.text = "0";

                GameManager.Instance.SortedNotes.RemoveAt(i);
                GameManager.Instance.NotesByLane[N.Lane].Remove(N);
                Destroy(N.gameObject);
                break;
            }

            if (N.transform.position.z > 1f)
                break;
        }
    }

    public MusicNote GetNearestNote(int LaneNum) => GameManager.Instance.NotesByLane[LaneNum][0];

    public void PlayStems()
    {
        foreach (KeyValuePair<StemType, AudioSource> KVP in LoadedStems)
        {
            if (KVP.Key != StemType.Backing)
                KVP.Value.timeSamples = _backingSource.timeSamples; // set time samples to be the exact same :+1:

            print($"Playing {KVP.Value.clip.name} ({KVP.Value.clip.length}s)!");
            KVP.Value.Play();
        }

        MusicSyncDeltaTime = Time.unscaledTime; // to sync note timing with input timing
    }

    public bool HasStem(StemType Stem) => LoadedStems.ContainsKey(Stem);
    public void ChangeVolume(StemType ST, float NewVol)
    {
        if (!HasStem(ST))
            return;

        LoadedStems[ST].volume = NewVol;
    }
}
