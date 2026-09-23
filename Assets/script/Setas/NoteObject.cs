using UnityEngine;
using System.Collections.Generic;

public class NoteObject : MonoBehaviour
{
    public bool canBePressed;
    public KeyCode keyToPress;
    public GameObject hitEffect, goodEffect, perfectEffect, missEffect;

    [Tooltip("Distância (mundo) até o ativador para contar como Perfect.")]
    public float perfectRange = 0.15f;
    [Tooltip("Distância (mundo) até o ativador para contar como Good. Acima disso = Normal.")]
    public float goodRange = 0.45f;
    [Tooltip("Rede de segurança: seta que passar disto ABAIXO do ativador conta como Miss sozinha.")]
    public float autoMissBelowActivator = 2f;

    // Ativador que a nota está tocando agora (definido no trigger).
    private Transform currentActivator;
    // Já foi resolvida (acerto ou erro)? Garante contagem exata de 1 por seta.
    private bool resolved;
    private SpriteRenderer spriteRenderer;

    // Registro estático por tecla para responder "tem outra nota apertável?" sem FindObjectsOfType.
    private static readonly Dictionary<KeyCode, HashSet<NoteObject>> registry = new Dictionary<KeyCode, HashSet<NoteObject>>();

    void Start()
    {
        // Cache de segurança: ativador mais próximo na mesma coluna (caso o trigger falhe).
        if (currentActivator == null)
        {
            float bestDx = float.MaxValue;
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Activator"))
            {
                float dx = Mathf.Abs(go.transform.position.x - transform.position.x);
                if (dx < bestDx)
                {
                    bestDx = dx;
                    currentActivator = go.transform;
                }
            }
        }
    }

    void OnEnable()
    {
        if (!registry.TryGetValue(keyToPress, out var set))
        {
            set = new HashSet<NoteObject>();
            registry[keyToPress] = set;
        }
        set.Add(this);
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>Há alguma seta NÃO resolvida visível em alguma câmera?</summary>
    public static bool AnyNoteVisible()
    {
        foreach (var pair in registry)
        {
            foreach (NoteObject n in pair.Value)
            {
                if (n != null && !n.resolved && n.gameObject.activeInHierarchy
                    && n.spriteRenderer != null && n.spriteRenderer.isVisible)
                    return true;
            }
        }
        return false;
    }

    void OnDisable()
    {
        if (registry.TryGetValue(keyToPress, out var set))
            set.Remove(this);
        if (canBePressed) canBePressed = false;
        currentActivator = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if (canBePressed)
            {
                Hit();
            }
            else if (!AnyNoteNearWithKey(keyToPress))
            {
                if (GameManager.instance != null)
                    GameManager.instance.WrongKeyPress();
            }
        }

        // Rede de segurança: seta que caiu muito abaixo do ativador sem resolver
        // (fora da pista/trigger) conta como Miss para o jogo nunca travar.
        // Só vale com a contagem finalizada (nunca pune durante o 3-2-1).
        if (!resolved && currentActivator != null
            && GameManager.instance != null && GameManager.instance.PodeTomarDano()
            && transform.position.y < currentActivator.position.y - autoMissBelowActivator)
        {
            resolved = true;
            canBePressed = false;
            GameManager.instance.NoteMissed();
            JudgmentFX.ShowMiss(transform.position);
            if (missEffect != null) Instantiate(missEffect, transform.position, missEffect.transform.rotation);
            gameObject.SetActive(false);
        }
    }

    void Hit()
    {
        if (resolved || GameManager.instance == null) return;
        resolved = true;

        // Distância real até o ativador. Fallback: posição Y (comportamento antigo)
        // para notas posicionadas manualmente sem trigger configurado.
        float error;
        if (currentActivator != null)
            error = Vector2.Distance(transform.position, currentActivator.position);
        else
            error = Mathf.Abs(transform.position.y - 1.04f);

        if (error <= perfectRange)
        {
            GameManager.instance.PerfectHit();
            JudgmentFX.ShowPerfect(transform.position);
            if (perfectEffect != null) Instantiate(perfectEffect, transform.position, perfectEffect.transform.rotation);
        }
        else if (error <= goodRange)
        {
            GameManager.instance.GoodHit();
            JudgmentFX.ShowGood(transform.position);
            if (goodEffect != null) Instantiate(goodEffect, transform.position, goodEffect.transform.rotation);
        }
        else
        {
            GameManager.instance.NormalHit();
            JudgmentFX.ShowNormal(transform.position);
            if (hitEffect != null) Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);
        }
        gameObject.SetActive(false);
    }

    bool AnyNoteNearWithKey(KeyCode key)
    {
        if (registry.TryGetValue(key, out var set))
        {
            foreach (var n in set)
            {
                if (n != null && n != this && n.canBePressed && n.gameObject.activeInHierarchy)
                    return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Activator"))
        {
            canBePressed = true;
            currentActivator = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Só conta Miss ao sair do MESMO ativador que ativou a nota.
        // (Ignora trigger de pista vizinha encostando de raspão.)
        if (other.CompareTag("Activator") && other.transform == currentActivator
            && gameObject.activeSelf && !resolved)
        {
            resolved = true;
            canBePressed = false;
            currentActivator = null;
            if (GameManager.instance != null)
                GameManager.instance.NoteMissed();
            JudgmentFX.ShowMiss(transform.position);
            if (missEffect != null) Instantiate(missEffect, transform.position, missEffect.transform.rotation);
        }
    }
}