using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Aura Fighter — tabela de músicas/BPM das fases.
/// Menu "Aura Fighter / Aplicar BPM das Fases": aplica o BPM certo em cada cena
/// (BeatScroller.beatTempo e Conductor.songBpm) sem você ir campo por campo.
///
/// ⚠️ Só o BPM/velocidade é automático. O ÁUDIO da fase ainda precisa ser
/// arrastado no AudioSource da cena (campo "Music Source" do MusicManager).
/// </summary>
public static class AplicarBPMFases
{
    public struct FaseInfo
    {
        public string cena;
        public string musica;
        public string inimigo;
        public float bpm;

        public FaseInfo(string cena, string musica, string inimigo, float bpm)
        {
            this.cena = cena;
            this.musica = musica;
            this.inimigo = inimigo;
            this.bpm = bpm;
        }
    }

    /// <summary>Tabela oficial das fases. Altere aqui quando trocar a música.</summary>
    public static readonly FaseInfo[] Tabela =
    {
        new FaseInfo("Tutorial",       "IT'S TV TIME (Forró)",                        "—",                    148f),
        new FaseInfo("Fase1",          "Chicago (Michael Jackson)",                   "—",                    100f),
        new FaseInfo("Fase2",          "Carioca Girls (Demais)",                      "cachorro caramelo",    125f),
        new FaseInfo("Fase3",          "Never Gonna Meow You Up",                    "gato",                 113f),
        new FaseInfo("Fase4",          "Troll Face Song",                             "troll face",           106f),
        new FaseInfo("Fase5",          "Stuck Inside (FNAF, versão forró)",          "Homem do ruim",        110f),
        new FaseInfo("Fase6",          "Megalovania Pisadinha",                       "sans",             150f), // aceita 150–160
        new FaseInfo("Fase7",          "Laurinha Costa - Six Seven (DJ Cabello & DJ Tchouzen)", "tralaleiro", 158f),
        new FaseInfo("BossFinal",      "Fermo Aura (DUPÊ)",                           "passarinho (Angry Birds)", 142f),
    };

    [MenuItem("Aura Fighter/Aplicar BPM das Fases")]
    public static void Aplicar()
    {
        // Aviso se houver cena com alteração não salva (seria perdida ao trocar de cena).
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            bool ok = EditorUtility.DisplayDialog("Aura Fighter",
                "A cena aberta tem alterações NÃO salvas e elas serão perdidas.\n\n" +
                "Quer salvar antes de aplicar os BPMs?", "Salvar e aplicar", "Cancelar");
            if (!ok) return;
            EditorSceneManager.SaveOpenScenes();
        }

        var relatorio = new List<string>();
        foreach (FaseInfo f in Tabela)
        {
            string path = "Assets/Scenes/" + f.cena + ".unity";
            if (!System.IO.File.Exists(path))
            {
                relatorio.Add(f.cena + ": cena não encontrada ❌");
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (!scene.IsValid() || !scene.path.EndsWith(f.cena + ".unity"))
            {
                relatorio.Add(f.cena + ": não consegui abrir ❌");
                continue;
            }

            BeatScroller scroller = Object.FindObjectOfType<BeatScroller>();
            Conductor conductor = Object.FindObjectOfType<Conductor>();

            if (scroller != null)
            {
                scroller.beatTempo = f.bpm;
                if (scroller.unitsPerSecond > 0f)
                    scroller.unitsPerSecond = 0f; // deixa o BPM mandar na velocidade
            }
            if (conductor != null) conductor.songBpm = f.bpm;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            relatorio.Add(f.cena + ": " + f.bpm + " BPM" +
                          (scroller != null ? "" : " (sem BeatScroller ⚠️)"));
        }

        EditorUtility.DisplayDialog("Aura Fighter",
            "BPM aplicado:\n\n" + string.Join("\n", relatorio.ToArray()) +
            "\n\nFalta só arrastar o áudio de cada fase no MusicManager.",
            "Fechar");
    }

    [MenuItem("Aura Fighter/Ver Tabela de Músicas")]
    public static void VerTabela()
    {
        var sb = new System.Text.StringBuilder();
        foreach (FaseInfo f in Tabela)
            sb.AppendLine(f.cena + " — " + f.musica + " (" + f.inimigo + ") — " + f.bpm + " BPM");

        EditorUtility.DisplayDialog("Aura Fighter — Músicas das Fases", sb.ToString(), "Fechar");
    }
}