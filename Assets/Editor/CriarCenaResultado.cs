using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Aura Fighter — cria a cena "Resultado" em 1 clique:
/// menu "Aura Fighter / Criar Cena Resultado".
/// Gera Assets/Scenes/Resultado.unity (câmera + ResultadoUI)
/// e registra no Build Settings.
/// </summary>
public static class CriarCenaResultado
{
    private const string ScenePath = "Assets/Scenes/Resultado.unity";

    [MenuItem("Aura Fighter/Criar Cena Resultado")]
    public static void Criar()
    {
        // Cena vazia.
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Câmera com fundo escuro.
        GameObject camGO = new GameObject("Main Camera", typeof(Camera));
        camGO.tag = "MainCamera";
        Camera cam = camGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.06f, 0.12f, 1f);
        camGO.transform.position = new Vector3(0f, 0f, -10f);

        // Objeto que monta a tela de resultado via código.
        new GameObject("ResultadoUI", typeof(ResultadoUI));

        EditorSceneManager.SaveScene(scene, ScenePath);

        // Registra no Build Settings (sem duplicar).
        var lista = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool existe = false;
        foreach (var s in lista)
        {
            if (s.path == ScenePath) { existe = true; break; }
        }
        if (!existe)
        {
            lista.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = lista.ToArray();
        }

        EditorUtility.DisplayDialog("Aura Fighter",
            "Cena Resultado criada em:\n" + ScenePath + "\n\nRegistrada no Build Settings. ✅",
            "Fechar");
    }

    [MenuItem("Aura Fighter/Preparar Tela de Level")]
    public static void PrepararLevel()
    {
        const string levelPath = "Assets/Scenes/Tela de level.unity";
        Scene scene = EditorSceneManager.OpenScene(levelPath);
        if (!scene.path.EndsWith("Tela de level.unity"))
        {
            EditorUtility.DisplayDialog("Aura Fighter",
                "Troca de cena cancelada. Reabra e tente de novo.", "Fechar");
            return;
        }

        LevelSelectUI mgr = Object.FindObjectOfType<LevelSelectUI>();
        if (mgr == null)
        {
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            GameObject host = canvas != null ? canvas.gameObject : new GameObject("LevelSelect");
            mgr = host.AddComponent<LevelSelectUI>();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Aura Fighter",
            "Tela de level preparada: travas aplicadas nos botões visuais. ✅",
            "Fechar");
    }
}
