using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AudioSource theMusic;
    public bool startPlaying;
    public BeatScroller theBS;
    public static GameManager instance;

    public int currentScore;
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;

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

    public bool forceDefeat;
    public string nextSceneName;

    public event System.Action OnNoteHit;
    public event System.Action OnNoteMissed;

    private bool resultAdvanceReady;
    private float resultShownTime;

    private KeyCode lastWrongKey;
    private int lastWrongFrame;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (scoreText != null) scoreText.text = "Score: 0";
        currentMultiplier = 1;

        if (multiplierThresholds == null || multiplierThresholds.Length == 0)
        {
            multiplierThresholds = new int[] { 2, 4, 6, 8 };
        }

        totalNotes = FindObjectsOfType<NoteObject>().Length;

        Aura.GetOrCreate();
        Vida.GetOrCreate().OnDeath += EndRound;

        if (SaveSystem.TemSave())
        {
            Aura.instance.SetTotal(SaveSystem.GetAura());
            Vida.instance.SetHP(SaveSystem.GetVida());
        }

        SaveSystem.SalvarJogo(Aura.instance.totalAura, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, Vida.instance.currentHP);
    }

    void Update()
    {
        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                startPlaying = true;
                if (theBS != null) theBS.hasStarted = true;
                if (theMusic != null) theMusic.Play();
            }
        }
        else
        {
            bool musicFinished = theMusic == null || !theMusic.isPlaying;
            if (musicFinished && resultsScreen != null)
            {
                EndRound();
            }

            if (resultAdvanceReady && !string.IsNullOrEmpty(nextSceneName)
                && Time.time - resultShownTime > 0.5f && Input.anyKeyDown)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    void EndRound()
    {
        if (resultsShown) return;
        resultsShown = true;
        resultShownTime = Time.time;

        if (Aura.instance != null) Aura.instance.AddAura(currentScore);
        if (theMusic != null && theMusic.isPlaying) theMusic.Stop();

        if (Aura.instance != null)
        {
            Vida v = Vida.instance;
            SaveSystem.SalvarJogo(Aura.instance.totalAura, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, v != null ? v.currentHP : 100);
        }

        if (forceDefeat)
        {
            if (resultsScreen != null) resultsScreen.SetActive(false);
            BuildDefeatUI();
        }
        else
        {
            if (resultsScreen != null) resultsScreen.SetActive(true);

            if (normalsText != null) normalsText.text = normalHits.ToString();
            if (goodsText != null) goodsText.text = goodHits.ToString();
            if (perfectsText != null) perfectsText.text = perfectHits.ToString();
            if (missesText != null) missesText.text = missedHits.ToString();

            float totalHit = normalHits + goodHits + perfectHits;
            float percentHit = totalNotes > 0 ? (totalHit / totalNotes) * 100f : 0f;

            if (percentHitText != null) percentHitText.text = percentHit.ToString("F1") + "%";

            string rankVal = "F";
            if (percentHit > 40) rankVal = "D";
            if (percentHit > 55) rankVal = "C";
            if (percentHit > 70) rankVal = "B";
            if (percentHit > 85) rankVal = "A";
            if (percentHit > 95) rankVal = "S";

            if (rankText != null) rankText.text = rankVal;
            if (finalScoreText != null) finalScoreText.text = currentScore.ToString();

            if (!string.IsNullOrEmpty(nextSceneName)) BuildContinuePrompt();
        }

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
        if (OnNoteHit != null) OnNoteHit();
    }

    public void GoodHit()
    {
        currentScore += scorePerGoodNote * currentMultiplier;
        NoteHit();
        goodHits++;
        if (OnNoteHit != null) OnNoteHit();
    }

    public void PerfectHit()
    {
        currentScore += scorePerPerfectNote * currentMultiplier;
        NoteHit();
        perfectHits++;
        if (OnNoteHit != null) OnNoteHit();
    }

    public void NoteMissed()
    {
        currentMultiplier = 1;
        multiplierTracker = 0;
        if (multiText != null) multiText.text = "Multiplier: x" + currentMultiplier;
        missedHits++;
        if (OnNoteMissed != null) OnNoteMissed();

        if (Vida.instance != null) Vida.instance.TakeDamage(Vida.instance.damagePerMiss);
    }

    public void WrongKeyPress()
    {
        if (lastWrongFrame == Time.frameCount) return;
        lastWrongFrame = Time.frameCount;

        if (Vida.instance != null) Vida.instance.TakeDamage(Vida.instance.damagePerMiss);
        if (OnNoteMissed != null) OnNoteMissed();
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

        CreateDefeatFill(canvasGO.transform);

        Text title = CreateDefeatText("Title", canvasGO.transform, new Vector2(0, 200), 96, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 0.2f, 0.2f, 1f));
        title.text = "DERROTA";

        Text sub = CreateDefeatText("Subtitle", canvasGO.transform, new Vector2(0, 100), 38, TextAnchor.MiddleCenter, FontStyle.Normal, Color.white);
        sub.text = "O Red Bird venceu a batalha... Você precisa treinar nas fases para ganhar aura!";

        Text score = CreateDefeatText("Score", canvasGO.transform, new Vector2(0, 30), 46, TextAnchor.MiddleCenter, FontStyle.Bold, new Color(1f, 0.85f, 0.2f, 1f));
        score.text = "Pontuação: " + currentScore;

        Text prompt = CreateDefeatText("Prompt", canvasGO.transform, new Vector2(0, -250), 28, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
        prompt.text = "Pressione Espaço para continuar ▼";
    }

    void BuildContinuePrompt()
    {
        GameObject canvasGO = CreateOverlayCanvas("ContinuePrompt");
        Text prompt = CreateDefeatText("Prompt", canvasGO.transform, new Vector2(0, -420), 28, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
        prompt.text = "Pressione Espaço para continuar ▼";
    }
}