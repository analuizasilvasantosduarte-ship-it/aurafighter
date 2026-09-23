using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Aura Fighter — efeitos de julgamento (PERFECT / GOOD / NORMAL / MISS).
/// Texto flutuante + faíscas na cor do julgamento, na posição da seta.
/// Tudo via código: funciona mesmo sem prefabs configurados nos notes.
/// </summary>
public class JudgmentFX : MonoBehaviour
{
    [Tooltip("Arraste a Thaleah Fat aqui para fonte pixel. Vazio = padrão.")]
    public Font popupFont;
    public int popupSize = 40;
    public int particlesPerBurst = 12;

    private static JudgmentFX instance;
    private Camera gameplayCam;
    private Sprite dotSprite;

    private class Popup
    {
        public GameObject go;
        public RectTransform rt;
        public Text text;
        public float life;
        public float maxLife = 0.6f;
        public Vector2 startPos;
    }

    private class Spark
    {
        public GameObject go;
        public RectTransform rt;
        public Image img;
        public Vector2 vel;
        public float life;
        public float maxLife;
    }

    private readonly List<Popup> popups = new List<Popup>();
    private readonly List<Spark> sparks = new List<Spark>();
    private readonly List<Spark> sparkPool = new List<Spark>();

    public static JudgmentFX GetOrCreate()
    {
        if (instance != null) return instance;
        instance = FindObjectOfType<JudgmentFX>();
        if (instance != null) return instance;
        GameObject go = new GameObject("JudgmentFX");
        instance = go.AddComponent<JudgmentFX>();
        instance.BuildCanvas();
        return instance;
    }

    public static void ShowPerfect(Vector3 worldPos) { GetOrCreate().Spawn("PERFECT!", DesignTokens.Colors.Success, worldPos); }
    public static void ShowGood(Vector3 worldPos) { GetOrCreate().Spawn("GOOD", DesignTokens.Colors.Good, worldPos); }
    public static void ShowNormal(Vector3 worldPos) { GetOrCreate().Spawn("NORMAL", DesignTokens.Colors.NeutralInfo, worldPos); }
    public static void ShowMiss(Vector3 worldPos) { GetOrCreate().Spawn("MISS", DesignTokens.Colors.Danger, worldPos); }

    /// <summary>MISS sem posição (tecla no vazio): aparece no centro da linha de julgamento.</summary>
    public static void ShowMissCenter()
    {
        JudgmentFX fx = GetOrCreate();
        Vector2 center = new Vector2(0f, 60f);
        fx.SpawnPopup("MISS", DesignTokens.Colors.Danger, center);
        fx.Burst(center, DesignTokens.Colors.Danger, 8);
    }

    /// <summary>Explosão de faíscas numa posição do mundo (ex: em cima do Carlos).</summary>
    public static void BurstWorld(Vector3 worldPos, Color color, int count)
    {
        JudgmentFX fx = GetOrCreate();
        fx.Burst(fx.WorldToAnchored(worldPos), color, count);
    }

    void BuildCanvas()
    {
        GameObject canvasGO = new GameObject("JudgmentCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        // Sem CanvasScaler: posições em pixels crus de tela.
        dotSprite = MakeDotSprite();
    }

    void Spawn(string label, Color color, Vector3 worldPos)
    {
        Vector2 anchored = WorldToAnchored(worldPos);
        SpawnPopup(label, color, anchored);
        Burst(anchored, color, particlesPerBurst);
    }

    Vector2 WorldToAnchored(Vector3 worldPos)
    {
        if (gameplayCam == null || !gameplayCam) gameplayCam = Camera.main;
        Vector2 screen = new Vector2(Screen.width / 2f, Screen.height / 2f);
        if (gameplayCam != null)
        {
            Vector3 sp = gameplayCam.WorldToScreenPoint(worldPos);
            screen = new Vector2(sp.x, sp.y);
        }
        return new Vector2(screen.x - Screen.width / 2f, screen.y - Screen.height / 2f);
    }

    void SpawnPopup(string label, Color color, Vector2 anchored)
    {
        Transform parent = transform.Find("JudgmentCanvas");
        GameObject go = new GameObject("Popup", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchored;
        rt.sizeDelta = new Vector2(600, 120);
        rt.localScale = Vector3.one * 1.6f;
        Text t = go.AddComponent<Text>();
        t.font = popupFont != null ? popupFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = popupSize;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.color = color;
        t.raycastTarget = false;
        t.text = label;
        popups.Add(new Popup { go = go, rt = rt, text = t, life = 0.6f, startPos = anchored });
    }

    void Burst(Vector2 anchored, Color color, int count)
    {
        Transform parent = transform.Find("JudgmentCanvas");
        for (int i = 0; i < count; i++)
        {
            Spark s = GetSpark(parent);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(150f, 450f);
            s.vel = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            s.maxLife = s.life = Random.Range(0.3f, 0.55f);
            float size = Random.Range(10f, 24f);
            s.rt.anchoredPosition = anchored;
            s.rt.sizeDelta = new Vector2(size, size);
            s.rt.localScale = Vector3.one;
            Color c = color;
            c.a = 1f;
            s.img.color = c;
            s.go.SetActive(true);
            sparks.Add(s);
        }
    }

    Spark GetSpark(Transform parent)
    {
        if (sparkPool.Count > 0)
        {
            Spark s = sparkPool[sparkPool.Count - 1];
            sparkPool.RemoveAt(sparkPool.Count - 1);
            return s;
        }
        GameObject go = new GameObject("Spark", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        Image img = go.AddComponent<Image>();
        img.sprite = dotSprite;
        img.raycastTarget = false;
        go.SetActive(false);
        return new Spark { go = go, rt = rt, img = img };
    }

    void Update()
    {
        float dt = Time.deltaTime;
        for (int i = popups.Count - 1; i >= 0; i--)
        {
            Popup p = popups[i];
            p.life -= dt;
            if (p.life <= 0f)
            {
                Destroy(p.go);
                popups.RemoveAt(i);
                continue;
            }
            float f = p.life / p.maxLife;
            p.rt.anchoredPosition = p.startPos + new Vector2(0f, (1f - f) * 120f);
            float s = 1f + 0.6f * f;
            p.rt.localScale = new Vector3(s, s, 1f);
            Color c = p.text.color;
            c.a = Mathf.Clamp01(f * 1.5f);
            p.text.color = c;
        }
        for (int i = sparks.Count - 1; i >= 0; i--)
        {
            Spark s = sparks[i];
            s.life -= dt;
            if (s.life <= 0f)
            {
                s.go.SetActive(false);
                sparkPool.Add(s);
                sparks.RemoveAt(i);
                continue;
            }
            s.vel.y -= 900f * dt;
            s.rt.anchoredPosition += s.vel * dt;
            Color c = s.img.color;
            c.a = Mathf.Clamp01(s.life / s.maxLife);
            s.img.color = c;
        }
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
