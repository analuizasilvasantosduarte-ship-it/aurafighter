using UnityEngine;

/// <summary>
/// Aura Fighter — cofre da aura geral (fonte única de verdade).
/// Toda aura ganha na fase entra por aqui e já é salva no PlayerPrefs.
/// Usa a mesma chave do SaveSystem ("AuraFighter_Aura"), então o save atual continua valendo.
/// </summary>
public static class AuraBank
{
    private const string Key = "AuraFighter_Aura";

    private static int cache;
    private static bool loaded;

    /// <summary>Total de aura guardado.</summary>
    public static int Total
    {
        get
        {
            if (!loaded)
            {
                cache = PlayerPrefs.GetInt(Key, 0);
                loaded = true;
            }
            return cache;
        }
    }

    /// <summary>Soma aura e salva. Retorna o novo total.</summary>
    public static int Add(int amount)
    {
        if (amount <= 0) return Total;
        cache = Total + amount;
        Save();
        return cache;
    }

    public static void Set(int value)
    {
        cache = Mathf.Max(0, value);
        Save();
    }

    public static void Reset()
    {
        cache = 0;
        Save();
    }

    /// <summary>Recarrega do PlayerPrefs (útil após ApagarSave).</summary>
    public static void Reload()
    {
        loaded = false;
    }

    private static void Save()
    {
        loaded = true;
        PlayerPrefs.SetInt(Key, cache);
        PlayerPrefs.Save();
    }
}
