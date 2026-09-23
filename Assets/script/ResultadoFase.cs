/// <summary>
/// Aura Fighter — carrega o resultado da fase até a cena "Resultado".
/// Preenchido pelo GameManager ao fim da fase e lido pelo ResultadoUI.
/// (Static sobrevive à troca de cena.)
/// </summary>
public static class ResultadoFase
{
    public static bool TemDados { get; private set; }
    public static string Fase = "";
    public static int Score;
    public static float Normal;
    public static float Good;
    public static float Perfect;
    public static float Miss;
    public static float Percent;
    public static float TotalNotas;
    public static string Rank = "F";
    public static int AuraGanha;

    public static void Registrar(string fase, int score, float normal, float good,
        float perfect, float miss, float percent, string rank, int auraGanha, float totalNotas)
    {
        Fase = fase;
        Score = score;
        Normal = normal;
        Good = good;
        Perfect = perfect;
        Miss = miss;
        Percent = percent;
        Rank = rank;
        AuraGanha = auraGanha;
        TotalNotas = totalNotas;
        TemDados = true;
    }

    public static void Limpar()
    {
        TemDados = false;
    }
}
