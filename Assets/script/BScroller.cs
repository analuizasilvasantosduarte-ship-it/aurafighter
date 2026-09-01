using UnityEngine;

public class BScroller : MonoBehaviour
{
    public float beatTempo; // Ex: 126.4f
    public bool hasStarted;

    void Start()
    {
        // Converte batidas por minuto em unidades por segundo
        beatTempo = beatTempo / 60f;
    }

    void Update()
    {
        if (hasStarted)
        {
            transform.position -= new Vector3(0f, beatTempo * Time.deltaTime, 0f);
        }
    }
}