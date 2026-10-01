using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker;
        [TextArea(2, 4)]
        public string text;
        public Sprite image;
        public Sprite textBoxSprite;
        public bool mostrarCartaz;

        [Tooltip("Mostra a imagem escolhida ocupando a tela inteira (tem prioridade sobre o cartaz).")]
        public bool mostrarTelaCheia;
        [Tooltip("Imagem de tela inteira desta linha. Vazio = usa a imagem padrão do DialogueSystem.")]
        public Sprite imagemTelaCheia;
        [Tooltip("Se ligado, a caixa de diálogo aparece por cima da imagem de tela cheia. Desligado = só a imagem, e Espaço avança.")]
        public bool textoSobreImagem;
    }

    public List<DialogueLine> lines = new List<DialogueLine>();
    public string nextSceneName;
    public float letterDelay = 0.03f;
    public Sprite background;
    public Sprite cartaz;
    public Sprite defaultTextBoxSprite;

    [Header("Imagem de tela cheia")]
    [Tooltip("Imagem de tela cheia usada quando a linha marca 'Mostrar imagem de tela cheia' mas não tem imagem própria. Ex: um desenho seu.")]
    public Sprite imagemTelaCheiaPadrao;
    [Tooltip("Desligado (padrão): a imagem fica centralizada mantendo a proporção, sem esticar. Ligado: a imagem é esticada até preencher a tela toda.")]
    public bool telaCheiaEsticar;
    public bool startOnAwake = true;

    private Image sceneImage;
    private Image posterImage;
    private Image fullscreenImage;
    private Image panelImage;
    private Text speakerText;
    private Text messageText;
    private Text continueText;
    private Text posterHint;

    private int currentLineIndex;
    private Coroutine typingRoutine;
    private bool canAdvance;
    private bool blinkRunning;
    private bool initialized;

    void Start()
    {
        if (!initialized && startOnAwake)
        {
            Iniciar();
        }
    }

    public void Iniciar()
    {
        if (initialized) return;

        gameObject.SetActive(true);
        initialized = true;
        currentLineIndex = 0;

        Transform old = transform.Find("CutsceneCanvas");
        if (old != null) Destroy(old.gameObject);

        BuildUI();

        if (lines.Count > 0)
        {
            ShowLine(0);
        }
    }

    void Update()
    {
        if (lines.Count == 0) return;

        bool advance = Input.GetKeyDown(KeyCode.Space)
                    || Input.GetKeyDown(KeyCode.Return)
                    || Input.GetMouseButtonDown(0);

        if (!advance) return;

        if (!canAdvance)
        {
            if (typingRoutine != null) StopCoroutine(typingRoutine);
            typingRoutine = null;
            messageText.text = lines[currentLineIndex].text;
            canAdvance = true;
            if (continueText != null) continueText.gameObject.SetActive(true);
            return;
        }

        currentLineIndex++;
        if (currentLineIndex < lines.Count)
        {
            ShowLine(currentLineIndex);
        }
        else
        {
            FinishCutscene();
        }
    }

void ShowLine(int index)
{
    canAdvance = false;
    if (continueText != null) continueText.gameObject.SetActive(false);
    if (posterHint != null) posterHint.gameObject.SetActive(false);

    DialogueLine line = lines[index];

    Sprite telaCheiaSprite = line.imagemTelaCheia != null ? line.imagemTelaCheia : imagemTelaCheiaPadrao;
    bool telaCheia = line.mostrarTelaCheia && telaCheiaSprite != null;
    bool poster = line.mostrarCartaz && cartaz != null;
    bool caixa = !poster && (!telaCheia || line.textoSobreImagem);

    if (posterImage != null)
    {
        posterImage.gameObject.SetActive(poster);
        if (poster) posterImage.sprite = cartaz;
    }

    if (fullscreenImage != null)
    {
        fullscreenImage.gameObject.SetActive(telaCheia);
        if (telaCheia)
        {
            fullscreenImage.sprite = telaCheiaSprite;
            fullscreenImage.preserveAspect = !telaCheiaEsticar;
            RectTransform telaRt = fullscreenImage.rectTransform;
            telaRt.anchorMin = Vector2.zero;
            telaRt.anchorMax = Vector2.one;
            telaRt.pivot = new Vector2(0.5f, 0.5f);
            telaRt.offsetMin = Vector2.zero;
            telaRt.offsetMax = Vector2.zero;
            telaRt.localScale = Vector3.one;
            fullscreenImage.color = Color.white;
        }
    }

    if (sceneImage != null)
    {
        if (telaCheia || poster)
        {
            sceneImage.gameObject.SetActive(false);
        }
        else if (line.image != null)
        {
            sceneImage.sprite = line.image;
            sceneImage.gameObject.SetActive(true);
        }
        else
        {
            sceneImage.gameObject.SetActive(false);
        }
    }

    if (panelImage != null)
    {
        panelImage.gameObject.SetActive(caixa);
        if (caixa) ApplyTextBoxSprite(panelImage, line.textBoxSprite != null ? line.textBoxSprite : defaultTextBoxSprite);
    }

    if (speakerText != null && caixa)
    {
        speakerText.text = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker;
        speakerText.gameObject.SetActive(speakerText.text.Trim().Length > 0);
    }

    if (messageText != null)
    {
        messageText.text = "";
        if (!caixa)
        {
            canAdvance = true;
            if (posterHint != null) posterHint.gameObject.SetActive(true);
        }
        else
        {
            typingRoutine = StartCoroutine(TypeLine(line.text));
        }
    }
}

    IEnumerator TypeLine(string fullText)
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (messageText != null) messageText.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(letterDelay);
        }
        typingRoutine = null;
        canAdvance = true;
        if (continueText != null) continueText.gameObject.SetActive(true);
    }

    void FinishCutscene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            gameObject.SetActive(false);
            initialized = false;
        }
    }

    void BuildUI()
    {
        GameObject canvasGO = new GameObject("CutsceneCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        Image bgImage = CreateImage("Background", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero);
        bgImage.raycastTarget = false;
        bgImage.color = new Color(0.06f, 0.06f, 0.12f, 1f);
        if (background != null) bgImage.sprite = background;

        sceneImage = CreateImage("SceneImage", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Vector2.zero);
        sceneImage.raycastTarget = false;
        sceneImage.preserveAspect = true;
        sceneImage.gameObject.SetActive(false);

        fullscreenImage = CreateImage("FullscreenImage", canvasGO.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        fullscreenImage.raycastTarget = false;
        fullscreenImage.preserveAspect = !telaCheiaEsticar;
        fullscreenImage.color = Color.white;
        fullscreenImage.gameObject.SetActive(false);

        posterImage = CreateImage("PosterImage", canvasGO.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(900, 760));
        posterImage.raycastTarget = false;
        posterImage.preserveAspect = true;
        posterImage.gameObject.SetActive(false);

        posterHint = CreateText("PosterHint", canvasGO.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 235), new Vector2(600, 30), DesignTokens.Type.Label, TextAnchor.LowerCenter, FontStyle.Italic);
        posterHint.text = "Pressione Espaço ▼";
        posterHint.color = DesignTokens.Colors.DialogueText;
        posterHint.gameObject.SetActive(false);

        Image panel = CreateImage("DialoguePanel", canvasGO.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 170), new Vector2(-80, 330));
        panel.raycastTarget = false;
        panelImage = panel;
        ApplyTextBoxSprite(panel, defaultTextBoxSprite);
        Outline panelOutline = panel.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = DesignTokens.Colors.DialogueBoxBorder;
        panelOutline.effectDistance = new Vector2(4f, -4f);

        speakerText = CreateText("Speaker", panel.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, -12), new Vector2(-40, 40), DesignTokens.Type.BodyM, TextAnchor.UpperLeft, FontStyle.Bold);
        speakerText.color = DesignTokens.Colors.DialogueText;

        messageText = CreateText("Message", panel.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(-120, -75), DesignTokens.Type.BodyL, TextAnchor.UpperLeft, FontStyle.Normal);
        messageText.color = DesignTokens.Colors.DialogueText;

        continueText = CreateText("Continue", panel.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-45, 12), new Vector2(200, 30), 24, TextAnchor.LowerRight, FontStyle.Italic);
        continueText.text = "Pressione Espaço ▼";
        continueText.gameObject.SetActive(false);

        if (!blinkRunning)
        {
            blinkRunning = true;
            StartCoroutine(BlinkContinue());
        }
    }

IEnumerator BlinkContinue()
{
        while (true)
        {
            bool hintBox = continueText != null && continueText.gameObject.activeSelf;
            bool hintPoster = posterHint != null && posterHint.gameObject.activeSelf;

            if (hintBox || hintPoster)
            {
                Color on = new Color(1, 1, 1, 0.9f);
                Color off = new Color(1, 1, 1, 0.25f);
                if (hintBox) continueText.color = on;
                if (hintPoster) posterHint.color = on;
                yield return new WaitForSeconds(0.45f);
                if (hintBox) continueText.color = off;
                if (hintPoster) posterHint.color = off;
                yield return new WaitForSeconds(0.45f);
            }
            else
            {
                yield return null;
            }
        }
    }

    Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        return go.AddComponent<Image>();
    }

    Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta, int fontSize, TextAnchor alignment, FontStyle style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;

        Text t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.fontStyle = style;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        return t;
    }

    void ApplyTextBoxSprite(Image panel, Sprite sprite)
    {
        if (panel == null) return;

        if (sprite != null)
        {
            panel.sprite = sprite;
            panel.color = Color.white;
            panel.type = Image.Type.Sliced;
            if (sprite.border == Vector4.zero) panel.type = Image.Type.Simple;
            panel.preserveAspect = false;
        }
        else
        {
            panel.sprite = null;
            panel.color = DesignTokens.Colors.DialogueBoxBg;
            panel.type = Image.Type.Simple;
            panel.preserveAspect = false;
        }
    }
}