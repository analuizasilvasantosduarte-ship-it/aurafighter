using UnityEngine;

public class DanceSway : MonoBehaviour
{
    public float swingAngle = 12f;
    public float swingSpeed = 1.5f;
    public float bobHeight = 0.08f;
    public float bobSpeed = 2f;
    public float pulseAmount = 0.06f;
    public float pulseSpeed = 4f;

    private Vector3 startPosition;
    private Vector3 startScale;

    void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
    }

    void Update()
    {
        float swing = Mathf.Sin(Time.time * swingSpeed * Mathf.PI * 2f);
        float bob = Mathf.Sin(Time.time * bobSpeed * Mathf.PI * 2f);
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * pulseAmount;

        transform.rotation = Quaternion.Euler(0f, 0f, swing * swingAngle);
        transform.position = startPosition + Vector3.up * (bob * bobHeight);
        transform.localScale = startScale * pulse;
    }
}