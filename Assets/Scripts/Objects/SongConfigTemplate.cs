using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Melanchall.DryWetMidi.Core;

public class SongDifficultyTemplate
{
    [JsonProperty("ds")] public int Drums;
    [JsonProperty("ba")] public int Bass;
    [JsonProperty("gr")] public int Lead;
    [JsonProperty("vl")] public int Vocals;
}

public class SongStemsConfigurationTemplate
{
    [JsonProperty("drums")] public string Drums;
    [JsonProperty("bass")] public string Bass;
    [JsonProperty("lead")] public string Lead;
    [JsonProperty("vocals")] public string Vocals;
    [JsonProperty("backing")] public string Backing;
}

public class SongConfigTemplate
{
    [JsonProperty("title")] public string Title;
    [JsonProperty("artist")] public string Artist;
    [JsonProperty("length")] public int SongLengthInSeconds;
    [JsonProperty("sid")] public string StartingInstrumentDrums;
    [JsonProperty("sib")] public string StartingInstrumentBass;
    [JsonProperty("sig")] public string StartingInstrumentLead;
    [JsonProperty("siv")] public string StartingInstrumentVocals;
    [JsonProperty("midi")] public string ChartFileName;
    [JsonProperty("art")] public string CoverImageFileName;
    [JsonProperty("diff")] public SongDifficultyTemplate Difficulties;
    [JsonProperty("stems")] public SongStemsConfigurationTemplate StemFileNames;
    [NonSerialized] public string SongDirectory;
    [NonSerialized] public MidiFile Chart;
    [NonSerialized] public Texture2D Cover;
}
