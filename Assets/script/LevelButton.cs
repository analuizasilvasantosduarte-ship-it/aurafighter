using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Aura Fighter — regra de liberação da fase (SEM mexer em visual).
/// O visual (liga/desliga botão + rótulo) é aplicado pelo LevelSelectUI
/// no BOTÃO QUE O JOGADOR CLICA. Este componente só guarda a config
/// e valida na hora do clique.
/// </summary>
public class LevelButton : MonoBehaviour
{
    [SerializeField]
    private string sceneName = "";

    [SerializeField]
    private int requiredPoints = 0;

    [Tooltip("Fase anterior obrigatória (nome da cena). Vazio = sem exigência.")]
    [SerializeField]
    private string previousPhase = "";

    public string Cena => sceneName;
    public int Requisito => requiredPoints;
    public string Anterior => previousPhase;

    /// <summary>Diz se a fase está liberada e o motivo do bloqueio (null = livre).</summary>
    public bool Liberada(out string motivo)
    {
        if (!SaveSystem.FaseConcluida(previousPhase))
        {
            motivo = string.IsNullOrEmpty(previousPhase) ? null : "termine " + previousPhase;
            if (motivo != null) return false;
        }
        int aura = SaveSystem.TemSave() ? SaveSystem.GetAura() : 0;
        if (aura < requiredPoints)
        {
            motivo = requiredPoints + " aura";
            return false;
        }
        motivo = null;
        return true;
    }

    public void Jogar()
    {
        if (!Liberada(out _)) return;
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
