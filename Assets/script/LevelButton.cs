using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [SerializeField]
    private string sceneName = "";

    [SerializeField]
    private int requiredPoints = 0;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        int pontos = SaveSystem.TemSave() ? SaveSystem.GetAura() : 0;
        if (button != null)
        {
            button.interactable = pontos >= requiredPoints;
        }
    }

    public void Jogar()
    {
        int pontos = SaveSystem.TemSave() ? SaveSystem.GetAura() : 0;
        if (pontos < requiredPoints)
        {
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}