using UnityEngine;

/// <summary>
/// Aura Fighter — dono único da música da fase.
/// Corrige: autoplay duplicado ("Audio Track" + theMusic tocando juntas),
/// Play() duplo reiniciando a música, fim de música detectado errado
/// (isPlaying puro) e pause sem compensação no relógio.
/// O GameManager cria/acha sozinho e liga o theMusic da cena.
/// </summary>
public class MusicManager : MonoBehaviour
{
    [Tooltip("Source que toca a música. Vazio = o theMusic do GameManager.")]
    public AudioSource source;

    [Header("Afinação")]
    [Tooltip("Tolerância em segundos para considerar a música terminada.")]
    public float finishTolerance = 0.15f;
    [Tooltip("Volume (0-1). Salvo automaticamente.")]
    [Range(0f, 1f)] public float volume = 1f;

    public bool IsPlaying => source != null && source.isPlaying;
    public bool HasStarted { get; private set; }
    public bool IsMuted { get; private set; }

    public float ClipLength => source != null && source.clip != null ? source.clip.length : 0f;

    /// <summary>Tempo da música em segundos (compensa pause).</summary>
    public float SongTime
    {
        get
        {
            if (!HasStarted || source == null || source.clip == null) return 0f;
            double t = AudioSettings.dspTime - dspStart - pausedTotal;
            if (paused) t -= AudioSettings.dspTime - pauseBegan;
            return Mathf.Max(0f, (float)t);
        }
    }

    /// <summary>Verdadeiro só quando a música TERMINOU de tocar (não em pause, não antes de começar).</summary>
    public bool HasFinished
    {
        get
        {
            if (!HasStarted || source == null || source.clip == null) return false;
            if (source.loop) return false;
            if (paused || source.isPlaying) return false;
            return SongTime >= ClipLength - finishTolerance;
        }
    }

    private double dspStart;
    private double pausedTotal;
    private double pauseBegan;
    private bool paused;

    private const string VolKey = "AuraFighter_MusicVolume";
    private const string MuteKey = "AuraFighter_MusicMute";

    public static MusicManager GetOrCreate()
    {
        MusicManager found = FindObjectOfType<MusicManager>();
        if (found != null) return found;
        return new GameObject("MusicManager").AddComponent<MusicManager>();
    }

    /// <summary>Liga ao source da cena e mata qualquer autoplay duplicado do mesmo clip.</summary>
    public void AttachSource(AudioSource s)
    {
        if (s != null) source = s;
        if (source == null) source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;

        if (source.clip != null)
        {
            foreach (AudioSource a in FindObjectsOfType<AudioSource>())
            {
                if (a != null && a != source && a.clip == source.clip)
                {
                    a.playOnAwake = false;
                    if (a.isPlaying) a.Stop();
                }
            }
        }

        volume = PlayerPrefs.GetFloat(VolKey, volume);
        source.volume = Mathf.Clamp01(volume);
        IsMuted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
        source.mute = IsMuted;
    }

    public void Play()
    {
        if (source == null || source.clip == null || paused) return;
        if (source.isPlaying) return; // nunca duplica nem reinicia
        source.Play();
        HasStarted = true;
        dspStart = AudioSettings.dspTime;
        pausedTotal = 0;
    }

    /// <summary>Agenda o início com precisão de sample (ideal p/ começar exato no fim da contagem).</summary>
    public void PlayDelayed(float delaySec)
    {
        if (source == null || source.clip == null || paused) return;
        if (source.isPlaying) return;
        HasStarted = true;
        pausedTotal = 0;
        dspStart = AudioSettings.dspTime + Mathf.Max(0f, delaySec);
        source.PlayScheduled(dspStart);
    }

    public void Stop()
    {
        if (source != null && source.isPlaying) source.Stop();
        HasStarted = false;
        paused = false;
    }

    public void Pause()
    {
        if (source == null || !source.isPlaying || paused) return;
        source.Pause();
        paused = true;
        pauseBegan = AudioSettings.dspTime;
    }

    public void Resume()
    {
        if (source == null || !paused) return;
        source.UnPause();
        pausedTotal += AudioSettings.dspTime - pauseBegan;
        paused = false;
    }

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (source != null) source.volume = volume;
        PlayerPrefs.SetFloat(VolKey, volume);
        PlayerPrefs.Save();
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        if (source != null) source.mute = IsMuted;
        PlayerPrefs.SetInt(MuteKey, IsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
}
