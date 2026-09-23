using UnityEngine;

/// <summary>
/// Aura Fighter — Conductor (relógio da música).
/// Mede o tempo da música com AudioSettings.dspTime para não dessincronizar
/// das notas quando há lag/variação de framerate.
/// Uso: arraste para a cena de gameplay e referencie o mesmo AudioSource do GameManager.
/// Se não houver referência, o BeatScroller continua funcionando no modo antigo.
/// </summary>
public class Conductor : MonoBehaviour
{
    [Tooltip("Mesmo AudioSource que toca a música da fase.")]
    public AudioSource musicSource;

    [Tooltip("BPM da música (ex: 126.4).")]
    public float songBpm = 126.4f;

    [Tooltip("Atraso/adianto de calibragem em segundos. Positivo = notas chegam antes.")]
    public float songOffsetSec = 0f;

    [Tooltip("Segundos de espera antes de começar a contar (contagem regressiva).")]
    public float startDelaySec = 0f;

    public double dspSongStart { get; private set; }
    public bool isPlaying { get; private set; }

    /// <summary>Tempo atual da música em segundos (0 = início).</summary>
    public float SongTime
    {
        get
        {
            if (!isPlaying) return 0f;
            return (float)(AudioSettings.dspTime - dspSongStart - startDelaySec) + songOffsetSec;
        }
    }

    public float SecPerBeat => 60f / Mathf.Max(1f, songBpm);

    void Awake()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
    }

    /// <summary>Chame quando a música começar (GameManager já faz isso se houver Conductor na cena).</summary>
    public void StartSong()
    {
        if (isPlaying) return;
        dspSongStart = AudioSettings.dspTime;
        isPlaying = true;
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();
    }

    public void StopSong()
    {
        isPlaying = false;
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PauseSong()
    {
        if (musicSource != null) musicSource.Pause();
    }

    public void UnpauseSong()
    {
        if (musicSource != null) musicSource.UnPause();
    }
}
