using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource theMusic;
    public bool startPlaying;
    public BeatScroller theBS;
    public Conductor conductor;
    public MusicManager musicMgr;
    public static GameManager instance;

    [Header("Contagem regressiva")]
    [Tooltip("Se ligado, a música e as notas SÓ começam depois do 3-2-1.")]
    public bool useCountdown = true;
    public CountdownManager countdown;

    public int currentScore;
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;

    [Header("Balanceamento")]
    public int healOnPerfect = 2;
    public int healOnGood = 1;
    public int healOnNormal = 0;
    [Tooltip("Tecla errada pune com dano? Desligue para modo treino/iniciante.")]
    public bool punishWrongKey = true;

    [Header("SFX (opcional — arraste clips na cena)")]
    public AudioSource sfxSource;
    public AudioClip hitSfx;
    public AudioClip missSfx;

    [Header("Faísca do Carlos")]
    [Tooltip("Acertos seguidos para disparar faísca no Carlos.")]
    public int sparkStreak = 4;
    [Tooltip("Faíscas ao bater a meta.")]
    public int sparkBurst = 40;
    [Tooltip("Faíscas por acerto enquanto segue na sequência.")]
    public int sparkSustain = 8;
    public int currentStreak;
    private Transform carlosTarget;

    public int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThresholds;

    public Text scoreText;
    public Text multiText;

    public float totalNotes;
    public float normalHits;
    public float goodHits;
    public float perfectHits;
    public float missedHits;

    public GameObject resultsScreen;
    public Text percentHitText, normalsText, goodsText, perfectsText, missesText, rankText, finalScoreText;

    private bool resultsShown;
    private bool defeated;

    public bool forceDefeat;
    public string nextSceneName;

    [Header("Tela de derrota")]
    [Tooltip("Fundo da derrota (ex: derrota.png). Arraste aqui. Vazio = fundo preto.")]
    public Sprite defeatBackground;
    [Tooltip("Título da derrota.")]
    public string defeatTitle = "DERROTA";
    [Tooltip("Subtítulo da derrota (editável por fase).")]
    [TextArea(2, 3)]
    public string defeatSubtitle = "O Red Bird venceu a batalha... Treine nas fases para ganhar aura!";
    [Tooltip("Texto do rodapé da derrota.")]
    public string defeatPrompt = "Espaço/R = tentar de novo   •   Esc = voltar às fases ▼";
    [Tooltip("Cena de resultado (criada pelo menu Aura Fighter/Criar Cena Resultado).")]
    public string resultSceneName = "Resultado";
    [Tooltip("Espera após a última seta antes de ir ao resultado.")]
    public float resultSceneDelay = 1.2f;
    [Header("Fim por tela vazia")]
    [Tooltip("Segundos sem seta visível (gameplay rolando) até mostrar o resultado.")]
    public float emptyScreenTimeout = 3f;
    [Tooltip("Cena ao sair da derrota (padrão: Tela de level).")]
    public string defeatReturnScene = "Tela de level";
    [Tooltip("Espera após a última seta antes de mostrar o resultado.")]
    public float resultDelay = 1f;

    public event System.Action OnNoteHit;
    public event System.Action OnNoteMissed;

    private bool resultAdvanceReady;
    private float resultShownTime;
    private float musicStartTime = -10f;
    private float allResolvedTime = -1f;
    private bool pendingInSceneResults;
    private float emptyScreenTimer;
    private bool everSawNotes;

    private int lastWrongFrame;

    [Header("Pause")]
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode retryKey = KeyCode.R;
    public KeyCode muteKey = KeyCode.M;
    private bool paused;
    private GameObject pauseOverlay;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
        if (Vida.instance != null) Vida.instance.OnDeath -= OnPlayerDeath;
    }

    void Start()
    {
        if (scoreText != null) scoreText.text = "Score: 0";
        // Zera TODO estado de partida (a cena pode ter salvo valores de um playtest no editor).
        currentScore = 0;
        currentMultiplier = 1;
        multiplierTracker = 0;
        normalHits = goodHits = perfectHits = missedHits = 0;
        totalNotes = 0;
        allResolvedTime = -1f;
        emptyScreenTimer = 0f;
        everSawNotes = false;
        currentStreak = 0;
        CarlosAnimator carlos = FindObjectOfType<CarlosAnimator>();
        carlosTarget = carlos != null ? carlos.transform : null;
        resultsShown = false;
        defeated = false;
        resultAdvanceReady = false;

        if (multiplierThresholds == null || multiplierThresholds.Length == 0)
        {
            multiplierThresholds = new int[] { 2, 4, 6, 8 };
        }

        totalNotes = FindObjectsOfType<NoteObject>().Length;

        Aura.GetOrCreate();
        Vida v = Vida.GetOrCreate();
        // Vida nova por tentativa: sempre começa cheia na fase.
        v.ResetVida();
        v.OnDeath -= OnPlayerDeath;
        v.OnDeath += OnPlayerDeath;

        if (conductor == null) conductor = FindObjectOfType<Conductor>();
        if (musicMgr == null) musicMgr = MusicManager.GetOrCreate();
        musicMgr.AttachSource(theMusic);
        if (countdown == null) countdown = CountdownManager.GetOrCreate();
        if (useCountdown && countdown != null && theBS != null)
            theBS.autoStartOnKey = false; // o scroll começa via countdown, não na tecla
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null || sfxSource == theMusic)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        if (SaveSystem.TemSave())
        {
            Aura.instance.SetTotal(AuraBank.Total);
        }

        SaveSystem.SalvarJogo(Aura.instance.totalAura, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, Vida.instance.currentHP);
    }

    void Update()
    {
        // Pause funciona durante o gameplay (não nas telas de resultado).
        if (startPlaying && !resultsShown && Input.GetKeyDown(pauseKey))
        {
            TogglePause();
            return;
        }
        if (paused)
        {
            // R recomeça a fase mesmo pausado.
            if (Input.GetKeyDown(retryKey))
            {
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
            return;
        }

        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                startPlaying = true;
                // A música é AGENDADA no sample exato do fim do 3-2-1; as notas soltam no callback.
                if (useCountdown && countdown != null)
                {
                    musicStartTime = Time.time + countdown.TotalDuration;
                    musicMgr.PlayDelayed(countdown.TotalDuration);
                    countdown.Begin(() => { if (theBS != null) theBS.Begin(); });
                }
                else
                    StartMusicAndScroll();
            }
        }
        else
        {
            if (Input.GetKeyDown(muteKey)) musicMgr.ToggleMute();
            bool musicFinished = musicMgr != null ? musicMgr.HasFinished : (theMusic == null || !theMusic.isPlaying);
            // Fallback: fim da música também encerra (caso sobre nota sem trigger, etc).
            if (musicFinished && resultsScreen != null && Time.time - musicStartTime > 1f)
            {
                EndRoundVictory();
            }

            // Principal: acabaram as setas (todas acertadas ou perdidas) → resultado.
            if (!resultsShown && totalNotes > 0)
            {
                float resolved = normalHits + goodHits + perfectHits + missedHits;
                if (resolved >= totalNotes)
                {
                    if (allResolvedTime < 0f) allResolvedTime = Time.time;
                    else if (Time.time - allResolvedTime >= resultDelay) EndRoundVictory();
                }
            }

            // Rede extra: tela sem seta visível por N segundos (gameplay rolando) → resultado.
            if (!resultsShown && Time.time >= musicStartTime)
            {
                if (NoteObject.AnyNoteVisible())
                {
                    everSawNotes = true;
                    emptyScreenTimer = 0f;
                }
                else if (everSawNotes)
                {
                    emptyScreenTimer += Time.deltaTime;
                    if (emptyScreenTimer >= emptyScreenTimeout) EndRoundVictory();
                }
            }

            if (resultAdvanceReady && Time.time - resultShownTime > 0.5f)
            {
                if (defeated)
                {
                    // Derrota: Espaço/R tenta de novo, Esc volta à seleção de fases.
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(retryKey))
                        UnityEngine.SceneManagement.SceneManager.LoadScene(
                            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                    else if (Input.GetKeyDown(pauseKey) && !string.IsNullOrEmpty(defeatReturnScene))
                        UnityEngine.SceneManagement.SceneManager.LoadScene(defeatReturnScene);
                }
                else if (pendingInSceneResults && Input.anyKeyDown)
                {
                    // Fallback (cena Resultado ainda não criada): qualquer tecla volta às fases.
                    if (!string.IsNullOrEmpty(defeatReturnScene))
                        UnityEngine.SceneManagement.SceneManager.LoadScene(defeatReturnScene);
                }
            }
        }
    }

    void OnPlayerDeath()
    {
        EndRoundDefeat();
    }

    /// <summary>Chamado quando a contagem termina (ou direto na tecla, sem contagem).</summary>
    void StartMusicAndScroll()
    {
        musicStartTime = Time.time;
        if (theBS != null) theBS.Begin();
        if (musicMgr != null) musicMgr.Play();
        else if (conductor != null) conductor.StartSong();
        else if (theMusic != null) theMusic.Play();
    }

    void EndRound()
    {
        EndRoundVictory();
    }

    void EndRoundVictory()
    {
        if (resultsShown) return;
        resultsShown = true;
        defeated = false;
        resultShownTime = Time.time;

        // Guarda a nova aura no cofre (salva na hora) e sincroniza o HUD da fase.
        int novaAura = AuraBank.Add(currentScore);
        if (Aura.instance != null) Aura.instance.SetTotal(novaAura);
        if (musicMgr != null) musicMgr.Stop();
        else if (theMusic != null && theMusic.isPlaying) theMusic.Stop();
        if (conductor != null) conductor.StopSong();
        if (theBS != null) theBS.StopScroll();

        {
            Vida v = Vida.instance;
            SaveSystem.SalvarJogo(novaAura, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, v != null ? v.currentHP : 100);
        }

        float totalHit = normalHits + goodHits + perfectHits;
        float percentHit = totalNotes > 0 ? (totalHit / totalNotes) * 100f : 0f;
        percentHit = Mathf.Clamp(percentHit, 0f, 100f);

        string rankVal = "F";
        if (percentHit > 40) rankVal = "D";
        if (percentHit > 55) rankVal = "C";
        if (percentHit > 70) rankVal = "B";
        if (percentHit > 85) rankVal = "A";
        if (percentHit > 95) rankVal = "S";

        if (rankText != null)
        {
            rankText.text = "Rank: " + rankVal;
            rankText.color = DesignTokens.Rank.Cor(percentHit);
        }
        if (finalScoreText != null) finalScoreText.text = "Score: " + currentScore;
        if (normalsText != null) normalsText.text = "Normal: " + normalHits;
        if (goodsText != null) goodsText.text = "Good: " + goodHits;
        if (perfectsText != null) perfectsText.text = "Perfect: " + perfectHits;
        if (missesText != null) missesText.text = "Miss: " + missedHits;
        if (percentHitText != null) percentHitText.text = percentHit.ToString("F1") + "%";

        string faseAtual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Salva melhor score/rank da fase.
        SaveSystem.SalvarResultado(faseAtual, currentScore, rankVal);

        // Marca a fase como concluída (libera a próxima na tela de level).
        SaveSystem.MarcarFaseConcluida(faseAtual);

        // Envia os dados para a cena Resultado (aura geral já atualizada acima).
        ResultadoFase.Registrar(faseAtual, currentScore, normalHits, goodHits,
            perfectHits, missedHits, percentHit, rankVal, currentScore, totalNotes);

        // Vai para a cena de resultado (com fallback se ela ainda não existir).
        Invoke("IrParaResultado", Mathf.Max(0f, resultSceneDelay));
    }

    void IrParaResultado()
    {
        if (!string.IsNullOrEmpty(resultSceneName)
            && Application.CanStreamedLevelBeLoaded(resultSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(resultSceneName);
            return;
        }
        // Fallback: mostra o resultado na própria fase.
        if (resultsScreen != null) resultsScreen.SetActive(true);
        BuildContinuePrompt();
        pendingInSceneResults = true;
        resultAdvanceReady = true;
    }

    void EndRoundDefeat()
    {
        if (resultsShown) return;
        resultsShown = true;
        defeated = true;
        resultShownTime = Time.time;

        if (musicMgr != null) musicMgr.Stop();
        else if (theMusic != null && theMusic.isPlaying) theMusic.Stop();
        if (conductor != null) conductor.StopSong();
        if (theBS != null) theBS.StopScroll();

        if (resultsScreen != null) resultsScreen.SetActive(false);
        BuildDefeatUI();
        resultAdvanceReady = true;
    }

    public void NoteHit()
    {
        if (currentMultiplier - 1 < multiplierThresholds.Length)
        {
            multiplierTracker++;
            if (multiplierThresholds[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
            }
        }

        if (multiText != null) multiText.text = "Multiplier: x" + currentMultiplier;
        if (scoreText != null) scoreText.text = "Score: " + currentScore;
    }

    public void NormalHit()
    {
        currentScore += scorePerNote * currentMultiplier;
        NoteHit();
        normalHits++;
        StreakUp();
        if (healOnNormal > 0 && Vida.instance != null) Vida.instance.Heal(healOnNormal);
        PlaySfx(hitSfx);
        if (OnNoteHit != null) OnNoteHit();
    }

    public void GoodHit()
    {
        currentScore += scorePerGoodNote * currentMultiplier;
        NoteHit();
        goodHits++;
        StreakUp();
        if (healOnGood > 0 && Vida.instance != null) Vida.instance.Heal(healOnGood);
        PlaySfx(hitSfx);
        if (OnNoteHit != null) OnNoteHit();
    }

    public void PerfectHit()
    {
        currentScore += scorePerPerfectNote * currentMultiplier;
        NoteHit();
        perfectHits++;
        StreakUp();
        if (healOnPerfect > 0 && Vida.instance != null) Vida.instance.Heal(healOnPerfect);
        PlaySfx(hitSfx);
        if (OnNoteHit != null) OnNoteHit();
    }

    /// <summary>Soma a sequência e solta faísca no Carlos ao bater/manter a meta.</summary>
    void StreakUp()
    {
        currentStreak++;
        if (carlosTarget == null || sparkStreak <= 0) return;
        if (currentStreak == sparkStreak)
            JudgmentFX.BurstWorld(carlosTarget.position, DesignTokens.Colors.AuraGold, sparkBurst);
        else if (currentStreak > sparkStreak && sparkSustain > 0)
            JudgmentFX.BurstWorld(carlosTarget.position, DesignTokens.Colors.AuraGold, sparkSustain);
    }

    void StreakReset()
    {
        currentStreak = 0;
    }

    /// <summary>Dano só vale com gameplay rolando: contagem finalizada e música começada.</summary>
    public bool PodeTomarDano()
    {
        if (!startPlaying || resultsShown || paused) return false;
        if (countdown != null && countdown.IsRunning) return false;
        if (Time.time < musicStartTime) return false;
        return true;
    }

    public void NoteMissed()
    {
        if (resultsShown) return;
        if (!PodeTomarDano()) return;
        StreakReset();
        currentMultiplier = 1;
        multiplierTracker = 0;
        if (multiText != null) multiText.text = "Multiplier: x" + currentMultiplier;
        missedHits++;
        PlaySfx(missSfx);
        if (OnNoteMissed != null) OnNoteMissed();

        if (Vida.instance != null) Vida.instance.TakeDamage(Vida.instance.damagePerMiss);
    }

    public void WrongKeyPress()
    {
        if (resultsShown) return;
        if (!PodeTomarDano()) return;
        StreakReset();
        if (lastWrongFrame == Time.frameCount) return;
        lastWrongFrame = Time.frameCount;

        if (punishWrongKey && Vida.instance != null) Vida.instance.TakeDamage(Vida.instance.damagePerMiss);
        JudgmentFX.ShowMissCenter();
        PlaySfx(missSfx);
        if (OnNoteMissed != null) OnNoteMissed();
    }

    void PlaySfx(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }

    public void TogglePause()
    {
        if (resultsShown) return;
        // Pause durante o 3-2-1 bagunça o agendamento da música — ignora.
        if (countdown != null && countdown.IsRunning) return;
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        if (musicMgr != null)
        {
            if (paused) musicMgr.Pause();
            else musicMgr.Resume();
        }
        else if (theMusic != null)
        {
            if (paused) theMusic.Pause();
            else theMusic.UnPause();
        }
        if (paused) ShowPauseOverlay();
        else HidePauseOverlay();
    }

    void ShowPauseOverlay()
    {
        if (pauseOverlay != null) { pauseOverlay.SetActive(true); return; }
        GameObject canvasGO = CreateOverlayCanvas("PauseScreen");
        pauseOverlay = canvasGO;
        CreateDefeatFill(canvasGO.transform);
        Text title = CreateDefeatText("Title", canvasGO.transform, new Vector2(0, 60), DesignTokens.Type.DisplayL, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);
        title.text = "PAUSADO";
        Text prompt = CreateDefeatText("Prompt", canvasGO.transform, new Vector2(0, -60), DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
        prompt.text = "Esc = continuar  •  R = recomeçar fase";
    }

    void HidePauseOverlay()
    {
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
    }

    GameObject CreateOverlayCanvas(string name)
    {
        GameObject canvasGO = new GameObject(name, typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();
        return canvasGO;
    }

    Image CreateDefeatFill(Transform parent)
    {
        GameObject go = new GameObject("Fill", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0, 0, 0, 0.85f);
        img.raycastTarget = true;
        return img;
    }

    Text CreateDefeatText(string name, Transform parent, Vector2 anchoredPosition, int fontSize, TextAnchor alignment, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = new Vector2(1200, 120);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.fontStyle = style;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        return t;
    }

    void BuildDefeatUI()
    {
        GameObject canvasGO = CreateOverlayCanvas("DefeatScreen");

        if (defeatBackground != null)
        {
            // Fundo do jogador em tela cheia + véu escuro p/ leitura.
            Image bg = CreateDefeatFill(canvasGO.transform);
            bg.sprite = defeatBackground;
            bg.color = Color.white;
            bg.preserveAspect = true;
            Image dim = CreateDefeatFill(canvasGO.transform);
            dim.color = new Color(0, 0, 0, 0.45f);
            dim.raycastTarget = false;
        }
        else
        {
            CreateDefeatFill(canvasGO.transform);
        }

        Text title = CreateDefeatText("Title", canvasGO.transform, new Vector2(0, 200), DesignTokens.Type.DisplayXL, TextAnchor.MiddleCenter, FontStyle.Bold, DesignTokens.Colors.Defeat);
        title.text = string.IsNullOrEmpty(defeatTitle) ? "DERROTA" : defeatTitle;

        Text sub = CreateDefeatText("Subtitle", canvasGO.transform, new Vector2(0, 100), 38, TextAnchor.MiddleCenter, FontStyle.Normal, Color.white);
        sub.text = defeatSubtitle;

        Text score = CreateDefeatText("Score", canvasGO.transform, new Vector2(0, 30), 46, TextAnchor.MiddleCenter, FontStyle.Bold, DesignTokens.Colors.AuraGoldText);
        score.text = "Pontuação: " + currentScore;

        Text prompt = CreateDefeatText("Prompt", canvasGO.transform, new Vector2(0, -250), 28, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
        prompt.text = defeatPrompt;
    }

    void BuildContinuePrompt()
    {
        GameObject canvasGO = CreateOverlayCanvas("ContinuePrompt");
        Text prompt = CreateDefeatText("Prompt", canvasGO.transform, new Vector2(0, -420), 28, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
        prompt.text = "Pressione qualquer tecla para voltar às fases ▼";
    }
}