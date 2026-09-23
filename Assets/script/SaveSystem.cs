using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class SaveSystem
{
    private const string SaveExists = "AuraFighter_SaveExists";
    private const string AuraKey = "AuraFighter_Aura";
    private const string PhaseKey = "AuraFighter_Fase";
    private const string VidaKey = "AuraFighter_Vida";
    private const string DoneListKey = "AuraFighter_DoneList";

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
        // Apaga também as fases concluídas (lista registrada).
        string lista = PlayerPrefs.GetString(DoneListKey, "");
        if (!string.IsNullOrEmpty(lista))
        {
            foreach (string f in lista.Split('|'))
            {
                if (!string.IsNullOrEmpty(f)) PlayerPrefs.DeleteKey(DoneKey(f));
            }
        }
        PlayerPrefs.DeleteKey(DoneListKey);
        PlayerPrefs.DeleteKey(AuraKey);
        PlayerPrefs.DeleteKey(PhaseKey);
        PlayerPrefs.DeleteKey(VidaKey);
        PlayerPrefs.DeleteKey(SaveExists);
        PlayerPrefs.Save();
    }

    // ---- Melhor resultado por fase (score + rank) ----

    private static string BestScoreKey(string fase) => "AuraFighter_Best_" + fase;
    private static string BestRankKey(string fase) => "AuraFighter_Rank_" + fase;

    private static readonly Dictionary<string, int> RankOrder =
        new System.Collections.Generic.Dictionary<string, int>
        {
            { "F", 0 }, { "D", 1 }, { "C", 2 }, { "B", 3 }, { "A", 4 }, { "S", 5 }
        };

    public static void SalvarResultado(string fase, int score, string rank)
    {
        if (string.IsNullOrEmpty(fase)) return;
        if (score > PlayerPrefs.GetInt(BestScoreKey(fase), 0))
            PlayerPrefs.SetInt(BestScoreKey(fase), score);

        string atual = PlayerPrefs.GetString(BestRankKey(fase), "F");
        int ordemAtual = RankOrder.ContainsKey(atual) ? RankOrder[atual] : 0;
        int ordemNova = RankOrder.ContainsKey(rank) ? RankOrder[rank] : 0;
        if (ordemNova > ordemAtual)
            PlayerPrefs.SetString(BestRankKey(fase), rank);
        PlayerPrefs.Save();
    }

    public static int GetBestScore(string fase)
    {
        return PlayerPrefs.GetInt(BestScoreKey(fase), 0);
    }

    public static string GetBestRank(string fase)
    {
        return PlayerPrefs.GetString(BestRankKey(fase), "-");
    }

    // ---- Progressão: fases concluídas ----

    private static string DoneKey(string fase) => "AuraFighter_Done_" + fase;

    /// <summary>Marca a fase como jogada/concluída (libera a próxima).</summary>
    public static void MarcarFaseConcluida(string fase)
    {
        if (string.IsNullOrEmpty(fase)) return;
        PlayerPrefs.SetInt(DoneKey(fase), 1);
        string lista = PlayerPrefs.GetString(DoneListKey, "");
        string[] itens = string.IsNullOrEmpty(lista) ? new string[0] : lista.Split('|');
        if (!itens.Contains(fase))
        {
            PlayerPrefs.SetString(DoneListKey, string.IsNullOrEmpty(lista) ? fase : lista + "|" + fase);
        }
        PlayerPrefs.Save();
    }

    /// <summary>Vazio = sem exigência (primeira fase sempre passa aqui).</summary>
    public static bool FaseConcluida(string fase)
    {
        if (string.IsNullOrEmpty(fase)) return true;
        return PlayerPrefs.GetInt(DoneKey(fase), 0) == 1;
    }
}