using UnityEngine;

public class BScroller : MonoBehaviour
{
    public float beatTempo; // Ex: 126.4f
    public bool hasStarted;

    private float scrollSpeed;
    private bool initialized;

    void Start()
    {
        // Converte batidas por minuto em unidades por segundo (uma vez só).
        if (!initialized)
        {
            scrollSpeed = beatTempo / 60f;
            initialized = true;
        }
    }

    void Update()
    {
        if (hasStarted)
        {
            transform.position -= new Vector3(0f, scrollSpeed * Time.deltaTime, 0f);
        }
    }
}