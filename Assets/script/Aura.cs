using UnityEngine;

public class Aura : MonoBehaviour
{
    public static Aura instance;

    public int totalAura;
    public UnityEngine.UI.Text auraText;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddAura(int amount)
    {
        if (amount <= 0) return;
        totalAura += amount;
        UpdateUI();
    }

    public void SetTotal(int value)
    {
        totalAura = Mathf.Max(0, value);
        UpdateUI();
    }

    public void ResetAura()
    {
        totalAura = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (auraText != null) auraText.text = "Aura: " + totalAura;
    }

    public static Aura GetOrCreate()
    {
        if (instance != null) return instance;
        return new GameObject("Aura").AddComponent<Aura>();
    }
}