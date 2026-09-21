using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string sceneName;

    public void Carregar()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void CarregarCena(string nome)
    {
        SceneManager.LoadScene(nome);
    }

    public void ProximaCena()
    {
        int index = SceneManager.GetActiveScene().buildIndex + 1;
        if (index < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(index);
        }
    }

    public void Continuar()
    {
        if (SaveSystem.TemSave())
        {
            SceneManager.LoadScene(SaveSystem.GetFase());
        }
        else
        {
            Carregar();
        }
    }

    public void NovoJogo()
    {
        SaveSystem.ApagarSave();
        if (Aura.instance != null) Aura.instance.ResetAura();
        if (Vida.instance != null) Vida.instance.ResetVida();
        Carregar();
    }

    public void SairJogo()
    {
        Application.Quit();
    }
}