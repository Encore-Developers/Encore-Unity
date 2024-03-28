using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Threading.Tasks;

public enum StemType
{
    Backing,
    Vocals,
    Lead,
    Bass,
    Drums
}

public class NotesManager : MonoSingleton<NotesManager>
{
    public override bool DontDestroy() => false;

    public Material SmasherHit, SmasherDefault;
    public float ScrollSpeed = 1.5f;
    public Dictionary<StemType, AudioSource> LoadedStems = new Dictionary<StemType, AudioSource>();
    AudioSource _backingSource = null;

    public void LoadStems(SongConfigTemplate Config)
    {
        foreach (FieldInfo F in typeof(SongStemsConfigurationTemplate).GetFields())
        {
            StemType StemT = Enum.Parse<StemType>(F.Name);
            string FileName = (string)F.GetValue(Config.StemFileNames);

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

    public void PlayStems()
    {
        foreach (KeyValuePair<StemType, AudioSource> KVP in LoadedStems)
        {
            if (KVP.Key != StemType.Backing)
                KVP.Value.timeSamples = _backingSource.timeSamples; // set time samples to be the exact same :+1:

            print($"Playing {KVP.Value.clip.name} ({KVP.Value.clip.length}s)!");
            KVP.Value.Play();
        }
    }

    public bool HasStem(StemType Stem) => LoadedStems.ContainsKey(Stem);
    public void ChangeVolume(StemType ST, float NewVol)
    {
        if (!HasStem(ST))
            return;

        LoadedStems[ST].volume = NewVol;
    }
}
