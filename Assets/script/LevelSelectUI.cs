using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Aura Fighter — aplica trava/rótulo nos BOTÕES VISUAIS da tela de level.
/// Como usar: menu "Aura Fighter / Preparar Tela de Level" (faz sozinho).
/// Funciona lendo a fiação real de clique: para cada botão da cena, acha o
/// LevelButton do seu onClick e aplica a regra naquele botão que o jogador vê.
/// Botões sem LevelButton (loja, config, voltar...) NÃO são tocados.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    private readonly Dictionary<Button, string> rotuloOriginal = new Dictionary<Button, string>();

    void Start()
    {
        AtualizarBotoes();
    }

    public void AtualizarBotoes()
    {
        foreach (Button btn in FindObjectsOfType<Button>())
        {
            LevelButton cfg = AcharConfig(btn);
            if (cfg == null) continue; // não é botão de fase: deixa quieto

            if (!rotuloOriginal.ContainsKey(btn))
            {
                Text t0 = btn.GetComponentInChildren<Text>();
                rotuloOriginal[btn] = t0 != null ? t0.text : cfg.Cena;
            }

            bool ok = cfg.Liberada(out string motivo);
            btn.interactable = ok;

            Text label = btn.GetComponentInChildren<Text>();
            if (label != null)
            {
                if (!ok)
                    label.text = cfg.Cena + " (" + motivo + ")";
                else
                {
                    string best = SaveSystem.GetBestRank(cfg.Cena);
                    int bestScore = SaveSystem.GetBestScore(cfg.Cena);
                    label.text = best != "-" ? cfg.Cena + " [" + best + " " + bestScore + "]" : rotuloOriginal[btn];
                }
            }
        }
    }

    static LevelButton AcharConfig(Button btn)
    {
        int n = btn.onClick.GetPersistentEventCount();
        for (int i = 0; i < n; i++)
        {
            LevelButton lb = btn.onClick.GetPersistentTarget(i) as LevelButton;
            if (lb != null && !string.IsNullOrEmpty(lb.Cena)) return lb;
        }
        return null;
    }
}
