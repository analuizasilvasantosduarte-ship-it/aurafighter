using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    [Tooltip("BPM (batidas por minuto). Convertido para unidades/segundo no Start.")]
    public float beatTempo;
    public bool hasStarted;

    [Tooltip("Velocidade em unidades por segundo (sobrescreve o cálculo por BPM se > 0).")]
    public float unitsPerSecond = 0f;

    [Tooltip("Opcional: Conductor da fase para medir o tempo real da música.")]
    public Conductor conductor;

    [Tooltip("Se ligado, qualquer tecla inicia a rolagem. O GameManager desliga quando usa contagem regressiva.")]
    public bool autoStartOnKey = true;

    private float scrollSpeed;
    private bool initialized;

    void Start()
    {
        // Evita dividir o BPM duas vezes se a cena for recarregada no mesmo objeto.
        if (!initialized)
        {
            scrollSpeed = unitsPerSecond > 0f ? unitsPerSecond : beatTempo / 60f;
            initialized = true;
        }
        if (conductor == null)
            conductor = FindObjectOfType<Conductor>();
    }

    void Update()
    {
        if (!hasStarted)
        {
            if (autoStartOnKey && Input.anyKeyDown)
            {
                hasStarted = true;
                if (conductor != null) conductor.StartSong();
            }
        }
        else
        {
            transform.position -= new Vector3(0f, scrollSpeed * Time.deltaTime, 0f);
        }
    }

    /// <summary>Inicia a rolagem (usado pelo GameManager/Conductor).</summary>
    public void Begin()
    {
        hasStarted = true;
    }

    /// <summary>Para a rolagem (derrota/vitória).</summary>
    public void StopScroll()
    {
        hasStarted = false;
    }
}