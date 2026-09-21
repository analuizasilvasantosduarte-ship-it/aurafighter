using UnityEngine;

public static class SaveSystem
{
    private const string SaveExists = "AuraFighter_SaveExists";
    private const string AuraKey = "AuraFighter_Aura";
    private const string PhaseKey = "AuraFighter_Fase";
    private const string VidaKey = "AuraFighter_Vida";

    public static bool TemSave()
    {
        return PlayerPrefs.GetInt(SaveExists, 0) == 1;
    }

    public static void SalvarJogo(int aura, string fase, int vida)
    {
        PlayerPrefs.SetInt(AuraKey, aura);
        PlayerPrefs.SetString(PhaseKey, fase);
        PlayerPrefs.SetInt(VidaKey, vida);
        PlayerPrefs.SetInt(SaveExists, 1);
        PlayerPrefs.Save();
    }

    public static int GetAura()
    {
        return PlayerPrefs.GetInt(AuraKey, 0);
    }

    public static string GetFase()
    {
        string fase = PlayerPrefs.GetString(PhaseKey, "Fase1");
        return string.IsNullOrEmpty(fase) ? "Fase1" : fase;
    }

    public static int GetVida()
    {
        return PlayerPrefs.GetInt(VidaKey, 100);
    }

    public static void ApagarSave()
    {
        PlayerPrefs.DeleteKey(AuraKey);
        PlayerPrefs.DeleteKey(PhaseKey);
        PlayerPrefs.DeleteKey(VidaKey);
        PlayerPrefs.DeleteKey(SaveExists);
        PlayerPrefs.Save();
    }
}