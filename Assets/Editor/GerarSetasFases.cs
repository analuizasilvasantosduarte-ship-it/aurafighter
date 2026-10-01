using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Aura Fighter — gera as setas de todas as fases automaticamente a partir do BPM
/// da música. Nada de arrastar seta por seta: escolhe a dificuldade e clica.
///
/// Como funciona a matemática:
///   a rolagem corre a "BPM/60 unidades por segundo" e começa junto com a música,
///   então uma seta que deve acertar em T segundos fica em
///       Y = Y_doAtivador + (BPM/60) * T
///   ou seja: batem no tempo certo da música sem você posicionar nada na mão.
///
/// Menu: Aura Fighter → Gerar Setas das Fases
/// </summary>
public class GerarSetasFases : EditorWindow
{
    private enum Padrao { Zigzag, Sequencial, Aleatorio }
    private enum Dificuldade { Facil = 1, Normal = 2, Dificil = 4, Insano = 6 }

    private static readonly string[] CenasFases =
        { "Tutorial", "Fase1", "Fase2", "Fase3", "Fase4", "Fase5", "Fase6", "Fase7", "BossFinal" };

    private Dificuldade dificuldade = Dificuldade.Normal;
    private Padrao padrao = Padrao.Zigzag;
    private float inicioSegundos = 3f;
    private float margemFinalSegundos = 5f;
    [Tooltip("DESLIGADO = mantém as setas já posicionadas e só acrescenta novas.")]
    private bool substituirExistentes = false;
    private bool saraivadaFinal = false;
    private float segundosSaraivada = 8f;
    private bool salvarChart = true;
    private int seed = 1234;
    private double chanceDupla = 0.12;
    private bool forcarSpriteUniversal = true;
    private string caminhoSprite = "Assets/Graphics/Arrow Right (1).png";

    [MenuItem("Aura Fighter/Gerar Setas das Fases")]
    public static void Abrir()
    {
        GetWindow<GerarSetasFases>("Gerar Setas");
    }

    void OnGUI()
    {
        GUILayout.Label("Sincronia com o BPM da música", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "As setas são posicionadas por tempo musical:\n" +
            "  Y = ativadorY + (BPM/60) * segundos\n" +
            "Assim elas batem no tempo certo da música.", MessageType.Info);

        dificuldade = (Dificuldade)EditorGUILayout.EnumPopup("Dificuldade", dificuldade);
        padrao = (Padrao)EditorGUILayout.EnumPopup("Padrão de setas", padrao);
        inicioSegundos = EditorGUILayout.FloatField("Começa em (s da música)", inicioSegundos);
        margemFinalSegundos = EditorGUILayout.FloatField("Para (s antes do fim)", margemFinalSegundos);
        substituirExistentes = EditorGUILayout.Toggle(
            new GUIContent("Substituir as setas existentes",
                "DESLIGADO = mantém as setas que você já posicionou (não move nem apaga) e só acrescenta novas."),
            substituirExistentes);
        chanceDupla = EditorGUILayout.Slider("Chance de seta dupla", (float)chanceDupla, 0f, 0.5f);
        seed = EditorGUILayout.IntField("Semente (padrão aleatório)", seed);

        saraivadaFinal = EditorGUILayout.Toggle("Saraivada final (dobrar no fim)", saraivadaFinal);
        if (saraivadaFinal)
            segundosSaraivada = EditorGUILayout.FloatField("Últimos segundos", segundosSaraivada);

        salvarChart = EditorGUILayout.Toggle("Salvar chart .txt (Assets/Charts)", salvarChart);

        GUILayout.Space(6);
        GUILayout.Label("Visual da seta", EditorStyles.boldLabel);
        forcarSpriteUniversal = EditorGUILayout.Toggle(
            new GUIContent("Forçar sprite universal", "Usa Arrow Right (1).png em todas as setas"),
            forcarSpriteUniversal);
        caminhoSprite = EditorGUILayout.TextField("Sprite (asset)", caminhoSprite);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Gerar na cena aberta", GUILayout.Height(32)))
        {
            string cena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (System.Array.IndexOf(CenasFases, cena) < 0)
            {
                EditorUtility.DisplayDialog("Gerar Setas",
                    "A cena aberta não é uma fase (" + cena + ").\nAbra Tutorial, Fase1..Fase7 ou BossFinal.", "OK");
            }
            else
            {
                int n = Gerar(cena, true);
                EditorUtility.DisplayDialog("Gerar Setas", "Cena " + cena + ": " + n + " setas geradas. ✅", "OK");
            }
        }

        if (GUILayout.Button("Gerar nas 9 fases de uma vez", GUILayout.Height(40)))
        {
            if (EditorSceneManager.GetActiveScene().isDirty &&
                !EditorUtility.DisplayDialog("Gerar Setas",
                    "A cena aberta tem alterações não salvas (serão perdidas). Salvar antes?", "Salvar e seguir", "Cancelar"))
                return;
            EditorSceneManager.SaveOpenScenes();

            var relatorio = new List<string>();
            foreach (string cena in CenasFases)
            {
                if (!File.Exists("Assets/Scenes/" + cena + ".unity")) { relatorio.Add(cena + ": cena ausente"); continue; }
                EditorSceneManager.OpenScene("Assets/Scenes/" + cena + ".unity", OpenSceneMode.Single);
                int n = Gerar(cena, false);
                relatorio.Add(cena + ": " + n + " setas");
            }
            EditorUtility.DisplayDialog("Gerar Setas — todas as fases",
                string.Join("\n", relatorio.ToArray()), "OK");
        }
    }

    /// <summary>Gera as setas da fase (mesma cena precisa estar aberta).</summary>
    int Gerar(string nomeCena, bool salvarAgora)
    {
        // 1) Velocidade da rolagem (BPM/60 ou unidade fixa) e BPM.
        BeatScroller bs = Object.FindObjectOfType<BeatScroller>();
        if (bs == null) { Debug.LogWarning("[Gerar Setas] " + nomeCena + " não tem BeatScroller."); return 0; }
        float velocidade = bs.CurrentSpeed;
        float bpm = bs.beatTempo;
        if (velocidade <= 0f) { Debug.LogWarning("[Gerar Setas] velocidade zero em " + nomeCena); return 0; }

        // 2) Ativadores (pistas) da esquerda para a direita.
        GameObject[] ativadores = GameObject.FindGameObjectsWithTag("Activator");
        if (ativadores.Length < 2) { Debug.LogWarning("[Gerar Setas] faltam ativadores em " + nomeCena); return 0; }
        System.Array.Sort(ativadores, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        int pistas = ativadores.Length;

        // 3) Setas atuais: exemplar por pista (para clonar sprite/visual) + lista para apagar.
        NoteObject[] atuais = Object.FindObjectsOfType<NoteObject>();
        var exemplares = new NoteObject[pistas];
        foreach (NoteObject n in atuais)
        {
            if (n == null) continue;
            int pista = PistaMaisProxima(n.transform.position.x, ativadores);
            if (exemplares[pista] == null) exemplares[pista] = n;
        }
        for (int i = 0; i < pistas; i++)
        {
            if (exemplares[i] != null) continue;
            foreach (NoteObject n in atuais) { exemplares[i] = n; break; } // fallback: qualquer uma
        }

        // 3b) Setas já posicionadas pelo usuário (não são mexidas nem podem ser duplicadas).
        var mantidas = new List<Vector2>(); // x = tempo musical, y = pista
        if (!substituirExistentes)
        {
            foreach (NoteObject n in atuais)
            {
                if (n == null) continue;
                int pista = PistaMaisProxima(n.transform.position.x, ativadores);
                float tempo = (n.transform.position.y - ativadores[pista].transform.position.y) / velocidade;
                mantidas.Add(new Vector2(tempo, pista));
            }
        }

        // 4) Duração da música da fase.
        float duracao = 60f;
        foreach (AudioSource src in Object.FindObjectsOfType<AudioSource>())
        {
            if (src != null && src.clip != null) { duracao = src.clip.length; break; }
        }

        // 5) Apaga as setas antigas (exemplares ficam até serem clonados, depois vão junto).
        if (substituirExistentes)
        {
            foreach (NoteObject n in atuais)
                if (n != null) Undo.DestroyObjectImmediate(n.gameObject);
        }

        // 6) Gera os tempos e posiciona cada seta no mundo.
        float intervaloBatida = 60f / Mathf.Max(1f, bpm);
        float densidade = (float)(int)dificuldade; // notas por batida
        float passo = intervaloBatida / Mathf.Max(0.25f, densidade);
        float fim = Mathf.Max(inicioSegundos + passo, duracao - margemFinalSegundos);
        float inicioSaraiva = fim - Mathf.Max(1f, segundosSaraivada);

        var rnd = new System.Random(seed);
        var chart = new List<string>();
        int idx = 0, ultimaPista = -1, criadas = 0;
        float tolerancia = passo * 0.6f;

        float t = inicioSegundos;
        while (t <= fim)
        {
            // Saraivada final: metade do intervalo = o dobro de setas no trecho.
            bool saraiva = saraivadaFinal && t >= inicioSaraiva;
            float passoAqui = saraiva ? passo * 0.5f : passo;

            int pista = EscolherPista(idx, pistas, ultimaPista, rnd);

            // Não sobrepõe as setas que você mesmo posicionou (mesma pista).
            bool livre = true;
            foreach (Vector2 m in mantidas)
            {
                if (m.y == pista && Mathf.Abs(m.x - t) < tolerancia) { livre = false; break; }
            }

            if (livre)
            {
                CriarSeta(exemplares[Mathf.Clamp(pista, 0, pistas - 1)], ativadores[pista], velocidade, t);
                chart.Add(t.ToString("F2") + "|" + pista);
                criadas++;
                ultimaPista = pista;
                idx++;

                // Setas duplas (duas pistas no mesmo tempo).
                if (rnd.NextDouble() < chanceDupla || (idx > 1 && idx % 6 == 0))
                {
                    int pista2 = (pista + 1 + rnd.Next(Mathf.Max(1, pistas - 1))) % pistas;
                    bool livre2 = true;
                    foreach (Vector2 m in mantidas)
                        if (m.y == pista2 && Mathf.Abs(m.x - t) < tolerancia) { livre2 = false; break; }
                    if (livre2)
                    {
                        CriarSeta(exemplares[Mathf.Clamp(pista2, 0, pistas - 1)], ativadores[pista2], velocidade, t);
                        chart.Add(t.ToString("F2") + "|" + pista2);
                        criadas++;
                    }
                }
            }

            t += passoAqui;
        }

        if (salvarChart)
        {
            string pasta = "Assets/Charts";
            if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);
            File.WriteAllText(pasta + "/" + nomeCena + ".txt",
                "# " + nomeCena + "  BPM " + bpm + "  gerado automaticamente\n" +
                "# formato: tempoSegundos|pista(0.." + (pistas - 1) + ")\n" +
                string.Join("\n", chart.ToArray()) + "\n");
            AssetDatabase.Refresh();
        }

        if (salvarAgora)
        {
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        Debug.Log("[Gerar Setas] " + nomeCena + " — BPM " + bpm + ", " + criadas + " setas novas" +
                  (substituirExistentes ? "" : " (mantidas " + mantidas.Count + " existentes)") +
                  ", velocidade " + velocidade.ToString("F2") + " u/s.");
        return criadas;
    }

    int EscolherPista(int idx, int pistas, int ultima, System.Random rnd)
    {
        switch (padrao)
        {
            case Padrao.Sequencial:
                return idx % pistas;
            case Padrao.Aleatorio:
            {
                int p;
                do { p = rnd.Next(pistas); } while (p == ultima && pistas > 1);
                return p;
            }
            default: // Zigzag: 0,1,2,3,2,1,0,1,2...
            {
                int ciclo = pistas * 2 - 2;
                if (ciclo <= 0) return idx % pistas;
                int i = idx % ciclo;
                return i < pistas ? i : ciclo - i;
            }
        }
    }

    void CriarSeta(NoteObject exemplar, GameObject ativador, float velocidade, float tempoSegundos)
    {
        if (exemplar == null || ativador == null) return;
        GameObject nova;
        if (PrefabUtility.IsPartOfPrefabInstance(exemplar.gameObject))
            nova = (GameObject)PrefabUtility.InstantiatePrefab(exemplar.gameObject);
        else
            nova = Object.Instantiate(exemplar.gameObject);
        Transform parent = exemplar.transform.parent;
        nova.transform.SetParent(parent, false);

        Vector3 alvo = ativador.transform.position;
        alvo.y = ativador.transform.position.y + velocidade * tempoSegundos;
        nova.transform.position = alvo;
        nova.SetActive(true);

        // Garante a tecla da pista (mesmo que o exemplar seja de outra pista).
        KeyCode tecla = TeclaDoAtivador(ativador);
        if (tecla != KeyCode.None)
        {
            NoteObject note = nova.GetComponent<NoteObject>();
            SerializedObject so = new SerializedObject(note);
            SerializedProperty p = so.FindProperty("keyToPress");
            if (p != null) { p.intValue = (int)tecla; so.ApplyModifiedPropertiesWithoutUndo(); }
        }

        // Visual: sprite único "Arrow Right (1)" girado para a direção da pista.
        if (forcarSpriteUniversal)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(caminhoSprite);
            if (sprite != null)
            {
                SpriteRenderer sr = nova.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = sprite;
            }
        }
        nova.transform.localRotation = Quaternion.Euler(0f, 0f, RotacaoParaTecla(tecla));

        Undo.RegisterCreatedObjectUndo(nova, "Gerar Seta");
    }

    /// <summary>Rotação (Z) da seta universal para cada direção.</summary>
    static float RotacaoParaTecla(KeyCode k)
    {
        switch (k)
        {
            case KeyCode.UpArrow: case KeyCode.W: return 90f;
            case KeyCode.DownArrow: case KeyCode.S: return -90f;
            case KeyCode.LeftArrow: case KeyCode.A: return 180f;
            case KeyCode.RightArrow: case KeyCode.D: return 0f;
            default: return 0f;
        }
    }

    /// <summary>Tecla do ativador, preferindo a seta do teclado (que é o que as notas usam).</summary>
    static KeyCode TeclaDoAtivador(GameObject ativador)
    {
        KeyCode primeira = KeyCode.None;
        foreach (Component c in ativador.GetComponents<Component>())
        {
            if (c == null) continue;
            SerializedObject so = new SerializedObject(c);
            SerializedProperty p = so.FindProperty("keyToPress");
            if (p == null || p.propertyType != SerializedPropertyType.Enum) continue;
            KeyCode k = (KeyCode)p.intValue;
            if (k == KeyCode.None) continue;
            // Setas do teclado são as que as notas respondem.
            if (k >= KeyCode.LeftArrow && k <= KeyCode.UpArrow) return k;
            if (primeira == KeyCode.None) primeira = k;
        }
        return primeira;
    }

    static int PistaMaisProxima(float x, GameObject[] ativadores)
    {
        int melhor = 0;
        float dist = float.MaxValue;
        for (int i = 0; i < ativadores.Length; i++)
        {
            float d = Mathf.Abs(ativadores[i].transform.position.x - x);
            if (d < dist) { dist = d; melhor = i; }
        }
        return melhor;
    }
}
