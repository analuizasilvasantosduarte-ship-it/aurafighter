using UnityEngine;

public class CarlosAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite normalSprite;
    public Sprite hitSprite;
    public Sprite missSprite;
    public float returnDelay = 0.3f;

    private bool subscribed;

    void Update()
    {
        if (!subscribed && GameManager.instance != null)
        {
            GameManager.instance.OnNoteHit += OnNoteHit;
            GameManager.instance.OnNoteMissed += OnNoteMissed;
            subscribed = true;
        }
    }

    void OnEnable()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        subscribed = false;
    }

    void OnDisable()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.OnNoteHit -= OnNoteHit;
            GameManager.instance.OnNoteMissed -= OnNoteMissed;
        }
        subscribed = false;
    }

    void OnNoteHit()
    {
        if (hitSprite != null && spriteRenderer != null) spriteRenderer.sprite = hitSprite;
        if (returnDelay > 0) Invoke("ResetSprites", returnDelay);
    }

    void OnNoteMissed()
    {
        if (missSprite != null && spriteRenderer != null) spriteRenderer.sprite = missSprite;
        if (returnDelay > 0) Invoke("ResetSprites", returnDelay);
    }

    void ResetSprites()
    {
        if (normalSprite != null && spriteRenderer != null) spriteRenderer.sprite = normalSprite;
    }
}