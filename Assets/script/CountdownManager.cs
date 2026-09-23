using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Aura Fighter — contagem regressiva 3 • 2 • 1 • VAI!
/// A música e as notas SÓ começam depois da contagem (ver GameManager).
/// Números grandes no centro da tela com explosões de partículas
/// vermelhas/laranjas a cada contagem. Tudo criado via código,
/// sem precisar mexer nas cenas.
/// </summary>
public class CountdownManager : MonoBehaviour
{
    [Header("Tempo")]
    [Tooltip("Segundos entre cada número.")]
    public float countInterval = 0.7f;
    [Tooltip("Segundos que o VAI! fica na tela.")]
    public float goDuration = 0.45f;

    [Header("Visual dos números")]
    [Tooltip("Arraste a Thaleah Fat aqui para usar a fonte pixel. Vazio = fonte padrão.")]
    public Font numberFont;
    public int numberSize = 220;
    public int hintSize = 32;

    [Header("Partículas (vermelhas / laranjas)")]
    public int particlesPerBurst = 36;
    public int goParticles = 60;
    public float minSpeed = 250f;
    public float maxSpeed = 750f;

    [Header("SFX opcional")]
    public AudioClip countBeep;
    public AudioClip goBeep;

    public bool IsRunning { get; private set; }

    /// <summary>Duração total do 3-2-1-VAI (usada p/ agendar a música no sample exato).</summary>
    public float TotalDuration => countInterval * 3f + goDuration;

    private GameObject root;
    private Text numberText;
    private AudioSource beepSource;
    private Sprite dotSprite;

    private float punchT = 1f;
    private const float PunchDuration = 0.45f;

    private class Particle
    {
        public GameObject go;
        public RectTransform rt;
        public Image img;
        public Vector2 vel;
        public float life;
        public float maxLife;
        public float size;
    }

    private readonly List<Particle> active = new List<Particle>();
    private readonly List<Particle> pool = new List<Particle>();

    private static readonly string[] PaletteHex = { "E60101", "D93A22", "F17300", "FFD200" };

    public static CountdownManager GetOrCreate()
    {
        CountdownManager found = FindObjectOfType<CountdownManager>();
        if (found != null) return found;
        return new GameObject("CountdownManager").AddComponent<CountdownManager>();
    }

    /// <summary>Inicia o 3-2-1 e chama onFinish quando termina.</summary>
    public void Begin(System.Action onFinish)
    {
        if (IsRunning) return;
        IsRunning = true;
        if (root == null) BuildUI();
        root.SetActive(true);
        StartCoroutine(Run(onFinish));
    }

    IEnumerator Run(System.Action onFinish)
    {
        yield return ShowStep("3", DesignTokens.GetColor(PaletteHex[0]), particlesPerBurst, countInterval, false);
        yield return ShowStep("2", DesignTokens.GetColor(PaletteHex[2]), particlesPerBurst, countInterval, false);
        yield return ShowStep("1", DesignTokens.Colors.AuraGold, particlesPerBurst, countInterval, false);
        yield return ShowStep("VAI!", DesignTokens.Colors.Success, goParticles, goDuration, true);
        root.SetActive(false);
        IsRunning = false;
        if (onFinish != null) onFinish();
    }

    IEnumerator ShowStep(string label, Color color, int burstCount, float hold, bool isGo)
    {
        numberText.text = label;
        numberText.color = color;
        punchT = 0f;
        Burst(burstCount);
        PlayBeep(isGo ? goBeep : countBeep);
        yield return new WaitForSeconds(hold);
    }

    void Update()
    {
        // Soco de escala no número (com overshoot).
        if (punchT < 1f && numberText != null)
        {
            punchT = Mathf.Min(1f, punchT + Time.deltaTime / PunchDuration);
            float s = 1f + 1.4f * (1f - EaseOutBack(punchT));
            numberText.rectTransform.localScale = new Vector3(s, s, 1f);
        }

        // Partículas de UI (posição em px do canvas, gravidade para baixo).
        float dt = Time.deltaTime;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Particle p = active[i];
            p.life -= dt;
            if (p.life <= 0f)
            {
                p.go.SetActive(false);
                pool.Add(p);
                active.RemoveAt(i);
                continue;
            }
            p.vel.y -= 1400f * dt;
            p.rt.anchoredPosition += p.vel * dt;
            float f = Mathf.Clamp01(p.life / p.maxLife);
            Color c = p.img.color;
            c.a = f;
            p.img.color = c;
            float s = 0.4f + 0.6f * f;
            p.rt.localScale = new Vector3(s, s, 1f);
        }
    }

    void Burst(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Particle p = GetParticle();
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(minSpeed, maxSpeed);
            p.vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            p.maxLife = p.life = Random.Range(0.5f, 0.9f);
            p.size = Random.Range(18f, 42f);
            p.rt.anchoredPosition = new Vector2(Random.Range(-30f, 30f), Random.Range(-30f, 30f));
            p.rt.sizeDelta = new Vector2(p.size, p.size);
            p.rt.localScale = Vector3.one;
            Color c = DesignTokens.GetColor(PaletteHex[Random.Range(0, PaletteHex.Length)]);
            c.a = 1f;
            p.img.color = c;
            p.go.SetActive(true);
            active.Add(p);
        }
    }

    Particle GetParticle()
    {
        if (pool.Count > 0)
        {
            Particle p = pool[pool.Count - 1];
            pool.RemoveAt(pool.Count - 1);
            return p;
        }
        GameObject go = new GameObject("Spark", typeof(RectTransform));
        go.transform.SetParent(root.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.sprite = dotSprite;
        img.raycastTarget = false;
        go.SetActive(false);
        return new Particle { go = go, rt = rt, img = img };
    }

    void PlayBeep(AudioClip clip)
    {
        if (clip != null && beepSource != null) beepSource.PlayOneShot(clip);
    }

    static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    void BuildUI()
    {
        root = new GameObject("Countdown", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Escurece levemente o fundo durante a contagem.
        Image dim = CreateImage("Dim", root.transform);
        dim.color = new Color(0f, 0f, 0f, 0.45f);

        numberText = CreateText("Number", root.transform, new Vector2(0, 40), new Vector2(1400, 420),
            numberSize, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);

        Text hint = CreateText("Hint", root.transform, new Vector2(0, -320), new Vector2(1400, 60),
            hintSize, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
        hint.text = "Prepare-se! A música vai começar...";

        dotSprite = MakeDotSprite();
        beepSource = gameObject.AddComponent<AudioSource>();
        root.SetActive(false);
    }

    Image CreateImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.AddComponent<Image>();
        img.raycastTarget = false;
        return img;
    }

    Text CreateText(string name, Transform parent, Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor align, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        Text t = go.AddComponent<Text>();
        t.font = numberFont != null ? numberFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.alignment = align;
        t.fontStyle = style;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        return t;
    }

    static Sprite MakeDotSprite()
    {
        const int s = 64;
        Texture2D tex = new Texture2D(s, s, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(s / 2f, s / 2f);
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / (s / 2f);
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f));
    }
}
