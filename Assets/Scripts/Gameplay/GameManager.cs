using Melanchall.DryWetMidi.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum SongDifficulty
{
    Easy,
    Medium,
    Hard,
    Expert
}

public class GameStatsTemplate
{
    public int PerfectHits = 0;
    public int LargestCombo = 0;
    public int CurrentCombo = 0;
    public int GoodHits = 0;
    public int Strikes = 0;
    public int Misses = 0;
    public int TotalNotes = 0;
    public int Stars = 0;
    public int Score = 0;
    public float Accuracy = 0; // calculated at the end of the game
    public bool GoldenStars = false;
}

public class GameManager : MonoSingleton<GameManager>
{
    public static int BaseScore = 30;
    public static float PerfectMultiplier = 1.2f;

    public TextMeshProUGUI ScoreText, ComboText;

    public SongConfigTemplate CurrentSongConfig;
    public GameStatsTemplate CurrentGameStats;
    public List<Smasher> ExpertSmashers;
    //IDisposable _pressThread, _releaseThread;

    public List<List<MusicNote>> NotesByLane = new List<List<MusicNote>>();
    public List<MusicNote> SortedNotes = new List<MusicNote>();
    
#if UNITY_EDITOR
    [Header("DEBUG")]
    public string dbgTestChartResourceFilename = "Songs/Chords/info.json";
    public SongDifficulty dbgTestSongDifficultyToLoad = SongDifficulty.Expert;
    public InstrumentType dbgTestInstrumentToPlay = InstrumentType.GUITAR;

    private void Start()
    {
        if (dbgTestChartResourceFilename != "")
        {
            LoadConfiguration(dbgTestChartResourceFilename, dbgTestInstrumentToPlay, dbgTestSongDifficultyToLoad);
            StartPlay();
        }
    }
#endif

    private unsafe void OnEnable()
    {
        Key[] ExpertKeys = new[] { Key.D, Key.F, Key.J, Key.K, Key.L };
        InputSystem.onEvent += (eventPtr, device) =>
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (!eventPtr.IsA<StateEvent>())
                return;

            if (!(device is Keyboard kb))
                return;

            for (int i = 0; i < ExpertKeys.Length; i++)
            {
                kb[ExpertKeys[i]].ReadValueFromEvent(eventPtr, out float K);
                Smasher CorrespondingSmasher = ExpertSmashers[i];
    
                if (K > 0 && !CorrespondingSmasher.HeldLastFrame)
                    CorrespondingSmasher.Smash(false);
                if (K<=0 && CorrespondingSmasher.HeldLastFrame)
                    CorrespondingSmasher.Smash(true);
                CorrespondingSmasher.HeldLastFrame = K > 0;
                CorrespondingSmasher.UpdateSmasherMaterial(K);

                //print($"{ExpertKeys[i]}: {(K == 1 ? "DOWN" : "")}");
            }
        };
    }

    //private void OnDisable()
    //{
        //_pressThread.Dispose();
        //_releaseThread.Dispose();
    //}

    public void LoadConfiguration(string ConfigPathRelativeToStreamingAssets, InstrumentType Instrument, SongDifficulty Diff)
    {
        foreach (AudioSource Source in Camera.main.gameObject.GetComponents<AudioSource>())
            Destroy(Source); // do not destroy the camera, only the audio sources

        foreach (var N in NotesByLane)
            foreach (var AN in N)
                Destroy(AN.gameObject); // DO destroy the music note gameobjects, they will be reinstantiated in the LoadNotes method

        NotesManager.Instance?.LoadedStems.Clear();
        NotesByLane.Clear();
        CurrentGameStats = new GameStatsTemplate();

        // LOAD CONFIGURATION
        CurrentSongConfig = JsonConvert.DeserializeObject<SongConfigTemplate>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, ConfigPathRelativeToStreamingAssets)));
        CurrentSongConfig.SongDirectory = Path.GetDirectoryName(Path.Combine(Application.streamingAssetsPath, ConfigPathRelativeToStreamingAssets));
        CurrentSongConfig.Chart = MidiFile.Read(Path.Combine(CurrentSongConfig.SongDirectory, CurrentSongConfig.ChartFileName));

        // LOAD SONG DATA
        NotesManager.Instance?.LoadStems(CurrentSongConfig);
        NotesManager.Instance?.LoadNotes(CurrentSongConfig, Instrument, Diff);
    }

    public void GoodHit()
    {
        CurrentGameStats.GoodHits++;
        CurrentGameStats.Score += BaseScore;

        CurrentGameStats.CurrentCombo++;

        if (CurrentGameStats.CurrentCombo > CurrentGameStats.LargestCombo)
            CurrentGameStats.LargestCombo = CurrentGameStats.CurrentCombo;

        ScoreText.text = CurrentGameStats.Score.ToString("#,#");
        ComboText.text = CurrentGameStats.CurrentCombo.ToString();
        //print("Good hit");
    }

    public void PerfectHit()
    {
        CurrentGameStats.PerfectHits++;
        CurrentGameStats.Score += Mathf.FloorToInt(BaseScore * PerfectMultiplier);

        CurrentGameStats.CurrentCombo++;

        if (CurrentGameStats.CurrentCombo > CurrentGameStats.LargestCombo)
            CurrentGameStats.LargestCombo = CurrentGameStats.CurrentCombo;

        ScoreText.text = CurrentGameStats.Score.ToString("#,#");
        ComboText.text = CurrentGameStats.CurrentCombo.ToString();
        //print("Perfect hit");
    }

    public void StartPlay()
    {
        // START PLAYING
        NotesManager.Instance?.PlayStems();
    }
}
