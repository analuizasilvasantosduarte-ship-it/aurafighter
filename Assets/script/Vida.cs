using UnityEngine;

public class Vida : MonoBehaviour
{
    public static Vida instance;

    public int maxHP = 100;
    public int currentHP;
    public int damagePerMiss = 5;
    public UnityEngine.UI.Text vidaText;

    public delegate void DeathHandler();
    public event DeathHandler OnDeath;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        currentHP = maxHP;
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            if (OnDeath != null) OnDeath();
        }
        UpdateUI();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateUI();
    }

    public void SetHP(int value)
    {
        currentHP = Mathf.Clamp(value, 1, maxHP);
        UpdateUI();
    }

    public void ResetVida()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (vidaText != null) vidaText.text = currentHP + "/" + maxHP;
    }

    public static Vida GetOrCreate()
    {
        if (instance != null) return instance;
        return new GameObject("Vida").AddComponent<Vida>();
    }
}