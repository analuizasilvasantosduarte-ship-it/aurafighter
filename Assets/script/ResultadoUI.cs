using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Aura Fighter — tela de Resultado (cena própria).
/// Lê os dados de ResultadoFase, mostra rank, acertos, score e a aura geral,
/// e volta para a seleção de fases com qualquer tecla.
/// Toda a UI é criada via código (a cena só precisa deste componente).
/// </summary>
public class ResultadoUI : MonoBehaviour
{
    [Tooltip("Para onde vai ao apertar qualquer tecla.")]
    public string returnScene = "Tela de level";

    private float shownTime;
    private Text promptText;

    void Start()
    {
        shownTime = Time.time;
        BuildUI();
    }

    void Update()
    {
        // Pisca o prompt.
        if (promptText != null)
        {
            float a = 0.55f + 0.45f * Mathf.Sin(Time.time * 4f);
            Color c = promptText.color;
            c.a = a;
            promptText.color = c;
        }

        if (Time.time - shownTime > 0.5f && Input.anyKeyDown)
        {
            if (!string.IsNullOrEmpty(returnScene))
                SceneManager.LoadScene(returnScene);
        }
    }

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("ResultadoCanvas", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Fundo.
        GameObject bgGO = new GameObject("BG", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        RectTransform bgRt = bgGO.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bg = bgGO.AddComponent<Image>();
        bg.color = DesignTokens.Colors.BlackBlue;
        bg.raycastTarget = false;

        bool tem = ResultadoFase.TemDados;

        string fase = tem ? ResultadoFase.Fase : "-";
        int score = tem ? ResultadoFase.Score : 0;
        float percent = tem ? ResultadoFase.Percent : 0f;
        string rank = tem ? ResultadoFase.Rank : "-";
        int auraTotal = AuraBank.Total;

        AddText(canvasGO.transform, "Title", "RESULTADO", 0, 340, DesignTokens.Type.DisplayL, TextAnchor.MiddleCenter, FontStyle.Bold, DesignTokens.Colors.AuraGoldText);
        AddText(canvasGO.transform, "Fase", tem ? ("Fase: " + fase) : "Sem dados — jogue uma fase!", 0, 265, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Normal, Color.white);

        Text rankT = AddText(canvasGO.transform, "Rank", rank, 0, 130, 120, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);
        if (tem) rankT.color = DesignTokens.Rank.Cor(percent);

        AddText(canvasGO.transform, "Percent", tem ? percent.ToString("F1") + "%" : "--", 0, 30, DesignTokens.Type.H1, TextAnchor.MiddleCenter, FontStyle.Bold, Color.white);
        AddText(canvasGO.transform, "Total", tem ? "Setas: " + (ResultadoFase.Normal + ResultadoFase.Good + ResultadoFase.Perfect) + " de " + ResultadoFase.TotalNotas : "", 0, -22, DesignTokens.Type.Label, TextAnchor.MiddleCenter, FontStyle.Normal, DesignTokens.Colors.GrayLight);

        AddText(canvasGO.transform, "Perfect", "Perfect: " + (tem ? ResultadoFase.Perfect : 0), 0, -55, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Normal, DesignTokens.Colors.Success);
        AddText(canvasGO.transform, "Good", "Good: " + (tem ? ResultadoFase.Good : 0), 0, -100, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Normal, DesignTokens.Colors.Good);
        AddText(canvasGO.transform, "Normal", "Normal: " + (tem ? ResultadoFase.Normal : 0), 0, -145, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Normal, DesignTokens.Colors.NeutralInfo);
        AddText(canvasGO.transform, "Miss", "Miss: " + (tem ? ResultadoFase.Miss : 0), 0, -190, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Normal, DesignTokens.Colors.Danger);

        AddText(canvasGO.transform, "Score", "Pontuação: " + score, 0, -270, DesignTokens.Type.H1, TextAnchor.MiddleCenter, FontStyle.Bold, DesignTokens.Colors.AuraGoldText);
        AddText(canvasGO.transform, "Aura", "Aura total: " + auraTotal + (tem && ResultadoFase.AuraGanha > 0 ? "  (+" + ResultadoFase.AuraGanha + ")" : ""), 0, -330, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Normal, DesignTokens.Colors.AuraGoldText);

        int best = !string.IsNullOrEmpty(fase) && fase != "-" ? SaveSystem.GetBestScore(fase) : 0;
        string bestRank = !string.IsNullOrEmpty(fase) && fase != "-" ? SaveSystem.GetBestRank(fase) : "-";
        AddText(canvasGO.transform, "Best", "Recorde da fase: " + best + "  [" + bestRank + "]", 0, -380, DesignTokens.Type.Label, TextAnchor.MiddleCenter, FontStyle.Italic, DesignTokens.Colors.GrayLight);

        promptText = AddText(canvasGO.transform, "Prompt", "Pressione qualquer tecla para voltar às fases ▼", 0, -470, DesignTokens.Type.BodyM, TextAnchor.MiddleCenter, FontStyle.Italic, Color.white);
    }

    Text AddText(Transform parent, string name, string content, float x, float y, int size, TextAnchor align, FontStyle style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(1400, 120);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.alignment = align;
        t.fontStyle = style;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        t.text = content;
        return t;
    }
}
