using Melanchall.DryWetMidi.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoSingleton<GameManager>
{
    public SongConfigTemplate CurrentSongConfig;
    public List<Smasher> ExpertSmashers;

#if UNITY_EDITOR
    [Header("DEBUG")]
    public string dbgTestChartResourceFilename = "Songs/Chords/info.json";

    private void Start()
    {
        if (dbgTestChartResourceFilename != "")
        {
            LoadConfiguration(dbgTestChartResourceFilename);
            StartPlay();
        }
    }
#endif

    public void LoadConfiguration(string ConfigPathRelativeToStreamingAssets)
    {
        // LOAD CONFIGURATION
        CurrentSongConfig = JsonConvert.DeserializeObject<SongConfigTemplate>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, ConfigPathRelativeToStreamingAssets)));
        CurrentSongConfig.SongDirectory = Path.GetDirectoryName(Path.Combine(Application.streamingAssetsPath, ConfigPathRelativeToStreamingAssets));
        CurrentSongConfig.Chart = MidiFile.Read(Path.Combine(CurrentSongConfig.SongDirectory, CurrentSongConfig.ChartFileName));

        // LOAD SONG DATA
        NotesManager.Instance?.LoadStems(CurrentSongConfig);
    }

    public void StartPlay()
    {
        // START PLAYING
        NotesManager.Instance?.PlayStems();
    }

    private void Update()
    {
        // excuse the hardcoded keybinds, will add settings later
        Key[] ExpertKeys = new[] { Key.D, Key.F, Key.J, Key.K, Key.L };
        for (int i = 0; i < ExpertSmashers.Count; i++)
        {
            Key Key = ExpertKeys[i];
            Smasher CorrespondingSmasher = ExpertSmashers[i];

            CorrespondingSmasher.IsHeld = Keyboard.current[Key].isPressed;
            CorrespondingSmasher.TappedThisFrame = Keyboard.current[Key].wasPressedThisFrame;
            CorrespondingSmasher.ReleasedThisFrame = Keyboard.current[Key].wasReleasedThisFrame;
        }
    }
}
