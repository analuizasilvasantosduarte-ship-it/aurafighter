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
    }

    public List<DialogueLine> lines = new List<DialogueLine>();
    public string nextSceneName;
    public float letterDelay = 0.03f;
    public Sprite background;
    public Sprite defaultTextBoxSprite;
    public bool startOnAwake = true;

    private Image sceneImage;
    private Image panelImage;
    private Text speakerText;
    private Text messageText;
    private Text continueText;

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

        DialogueLine line = lines[index];

        if (speakerText != null)
        {
            speakerText.text = string.IsNullOrEmpty(line.speaker) ? "" : line.speaker;
            speakerText.gameObject.SetActive(speakerText.text.Trim().Length > 0);
        }

        if (sceneImage != null)
        {
            if (line.image != null)
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
            ApplyTextBoxSprite(panelImage, line.textBoxSprite != null ? line.textBoxSprite : defaultTextBoxSprite);
        }

        if (messageText != null)
        {
            messageText.text = "";
            typingRoutine = StartCoroutine(TypeLine(line.text));
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
        sceneImage.gameObject.SetActive(false);

        Image panel = CreateImage("DialoguePanel", canvasGO.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, 170), new Vector2(-80, 330));
        panel.raycastTarget = false;
        panelImage = panel;
        ApplyTextBoxSprite(panel, defaultTextBoxSprite);

        speakerText = CreateText("Speaker", panel.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(0, -12), new Vector2(-40, 40), 28, TextAnchor.UpperLeft, FontStyle.Bold);
        speakerText.color = new Color(1f, 0.85f, 0.2f, 1f);

        messageText = CreateText("Message", panel.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(0, -15), new Vector2(-120, -75), 34, TextAnchor.UpperLeft, FontStyle.Normal);

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
            if (continueText != null && canAdvance)
            {
                continueText.color = new Color(1, 1, 1, 0.9f);
                yield return new WaitForSeconds(0.45f);
                continueText.color = new Color(1, 1, 1, 0.25f);
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
            panel.color = new Color(0, 0, 0, 0.82f);
            panel.type = Image.Type.Simple;
            panel.preserveAspect = false;
        }
    }
}