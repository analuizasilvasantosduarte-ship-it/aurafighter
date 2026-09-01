using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed; // Sincronizado com o speed do ChartLoader

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }
}