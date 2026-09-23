using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Aura Fighter — mostra a aura geral na tela de level ("Tela de level").
/// Como usar: selecione qualquer objeto da cena Tela de level,
/// Add Component → AuraLevelDisplay, e arraste o Text da aura no campo
/// (se não arrastar, ele procura um Text chamado *aura* ou cria um selo no topo).
/// </summary>
public class AuraLevelDisplay : MonoBehaviour
{
    [Tooltip("Text que mostra a aura. Vazio = procura/cria sozinho.")]
    public Text auraText;

    [Tooltip("{0} vira o total. Ex: \"Aura: {0}\".")]
    public string formato = "Aura: {0}";

    void Start()
    {
        Refresh();
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (auraText == null) auraText = FindAuraText();
        if (auraText == null) auraText = CreateFallbackLabel();
        auraText.text = string.Format(formato, AuraBank.Total);
        auraText.color = DesignTokens.Colors.AuraGoldText;
    }

    Text FindAuraText()
    {
        foreach (Text t in FindObjectsOfType<Text>())
        {
            if (t != null && t.name.ToLower().Contains("aura")) return t;
        }
        return null;
    }

    Text CreateFallbackLabel()
    {
        GameObject canvasGO = new GameObject("AuraDisplay", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject go = new GameObject("AuraTotal", typeof(RectTransform));
        go.transform.SetParent(canvasGO.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -30f);
        rt.sizeDelta = new Vector2(800, 80);
        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 48;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false;
        return t;
    }
}
