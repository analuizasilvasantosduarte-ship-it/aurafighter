using UnityEngine;

/// <summary>
/// Aura Fighter — Design System.
/// Centraliza cores, tipografia, espaçamentos e ranks do jogo.
/// Documentação completa em DESIGN_SYSTEM.md (raiz do projeto).
/// </summary>
public static class DesignTokens
{
    /// <summary>Cria uma cor a partir de hex (aceita "RRGGBB" ou "#RRGGBB" e AA sufixo opcional).</summary>
    public static Color GetColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.white;

        hex = hex.TrimStart('#');
        if (hex.Length == 6) hex += "FF";
        if (hex.Length != 8) return Color.white;

        byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
        byte a = System.Convert.ToByte(hex.Substring(6, 2), 16);
        return new Color32(r, g, b, a);
    }

    /// <summary>Cores do núcleo da marca.</summary>
    public static class Colors
    {
        // ---- Núcleo da marca (seção 2.1) ----
        public static readonly Color AuraGold        = GetColor("FFD200");
        public static readonly Color AuraGoldText    = GetColor("FFD933");
        public static readonly Color AuraViolet      = GetColor("4F119A");
        public static readonly Color AuraDeepPurple  = GetColor("2C004B");
        public static readonly Color AuraEmber       = GetColor("D93A22");
        public static readonly Color AuraEmberDark   = GetColor("B6240E");
        public static readonly Color AuraLavender    = GetColor("E0C7FF");

        // ---- Semânticos (seção 2.2) ----
        public static readonly Color Success         = GetColor("18BE03"); // Perfect
        public static readonly Color Good            = GetColor("F1E603"); // Good
        public static readonly Color NeutralInfo      = GetColor("035AD9"); // Normal
        public static readonly Color Danger          = GetColor("E60101"); // Miss
        public static readonly Color ButtonRed       = GetColor("FF0000");
        public static readonly Color ButtonBlue      = GetColor("003CFF");
        public static readonly Color ButtonGreen     = GetColor("0E9000");
        public static readonly Color Defeat          = GetColor("FF3333"); // título DERROTA

        // ---- Neutros (seção 2.3) ----
        public static readonly Color Black           = GetColor("000000");
        public static readonly Color BlackBlue       = GetColor("0F0F1F"); // fundo de cutscene
        public static readonly Color Charcoal        = GetColor("2C2C2C");
        public static readonly Color CharcoalLight   = GetColor("3D3D3D");
        public static readonly Color Gray            = GetColor("555555");
        public static readonly Color GrayLight       = GetColor("909090");
        public static readonly Color GrayLighter     = GetColor("E0E0E0");
        public static readonly Color White           = Color.white;

        /// <summary>Dimmer da tela de derrota (rgba(0,0,0,0.85)).</summary>
        public static readonly Color Overlay = new Color(0f, 0f, 0f, 0.85f);

        // ---- Pistas do ritmo (seção 2.4) ----
        public static readonly Color Lane1           = GetColor("003CFF"); // azul
        public static readonly Color Lane2           = GetColor("FFD200"); // amarela
        public static readonly Color Lane3           = GetColor("FF0000"); // vermelha
        public static readonly Color Lane4           = GetColor("0E9000"); // verde

        /// <summary>Cor da vida quando está baixa (&lt; 25%).</summary>
        public static readonly Color VidaBaixa       = Danger;

        /// <summary>Cor do rank por índice 0..5 (S, A, B, C, D, F).</summary>
        public static readonly Color[] RankColors =
        {
            AuraGold,    // S
            Success,     // A
            ButtonBlue,  // B
            Good,        // C
            GrayLight,   // D
            Danger       // F
        };
    }

    /// <summary>Escala tipográfica (Unity legado usa LegacyRuntime.ttf nas UIs dinâmicas).</summary>
    public static class Type
    {
        public const int DisplayXL = 96; // títulos de tela ("DERROTA")
        public const int DisplayL  = 64; // títulos de fase / rank
        public const int H1        = 48; // pontuação, ranking final
        public const int H2        = 36; // subtítulos
        public const int BodyL     = 34; // mensagem de diálogo
        public const int BodyM     = 28; // speaker, prompts
        public const int Label     = 24; // dicas / rótulos pequenos
    }

    /// <summary>Espaçamento base de 4 px (seção 4).</summary>
    public static class Space
    {
        public const float S1 = 4f;
        public const float S2 = 8f;
        public const float S3 = 12f;
        public const float S4 = 16f;
        public const float S6 = 24f;
        public const float S8 = 32f;
        public const float S12 = 48f;
        public const float S14 = 80f;  // margem lateral segura
        public const float S16 = 64f;
        public const float S20 = 120f;
    }

    /// <summary>Ranks e limites de acerto (mesma lógica de GameManager.EndRound).</summary>
    public static class Rank
    {
        public const int    PerfectPoints = 150;
        public const int    GoodPoints    = 125;
        public const int    NormalPoints  = 100;

        public const float  ThresholdS = 95f;
        public const float  ThresholdA = 85f;
        public const float  ThresholdB = 70f;
        public const float  ThresholdC = 55f;
        public const float  ThresholdD = 40f;

        /// <summary>Retorna a cor do rank dado o percentual de acerto (0–100).</summary>
        public static Color Cor(float percentHit)
        {
            if (percentHit > ThresholdS) return Colors.RankColors[0]; // S
            if (percentHit > ThresholdA) return Colors.RankColors[1]; // A
            if (percentHit > ThresholdB) return Colors.RankColors[2]; // B
            if (percentHit > ThresholdC) return Colors.RankColors[3]; // C
            if (percentHit > ThresholdD) return Colors.RankColors[4]; // D
            return Colors.RankColors[5];                              // F
        }
    }
}