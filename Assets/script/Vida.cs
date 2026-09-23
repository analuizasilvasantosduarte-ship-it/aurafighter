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

    public delegate void DamageHandler(int amount, int current, int max);
    public event DamageHandler OnDamaged;
    public event System.Action OnHealed;

    private bool isDead;

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

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            isDead = true;
            if (OnDamaged != null) OnDamaged(amount, currentHP, maxHP);
            UpdateUI();
            if (OnDeath != null) OnDeath();
            return;
        }
        if (OnDamaged != null) OnDamaged(amount, currentHP, maxHP);
        UpdateUI();
    }

    public void Heal(int amount)
    {
        if (isDead || amount <= 0) return;
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        if (OnHealed != null) OnHealed();
        UpdateUI();
    }

    public void SetHP(int value)
    {
        currentHP = Mathf.Clamp(value, 0, maxHP);
        isDead = currentHP <= 0;
        UpdateUI();
    }

    public void ResetVida()
    {
        currentHP = maxHP;
        isDead = false;
        UpdateUI();
    }

    public float Fraction => maxHP > 0 ? (float)currentHP / maxHP : 0f;

    void UpdateUI()
    {
        if (vidaText != null)
        {
            vidaText.text = currentHP + "/" + maxHP;
            // Vida baixa destaca em vermelho (DesignTokens).
            vidaText.color = Fraction < 0.25f ? DesignTokens.Colors.VidaBaixa : Color.white;
        }
    }

    public static Vida GetOrCreate()
    {
        if (instance != null) return instance;
        return new GameObject("Vida").AddComponent<Vida>();
    }
}