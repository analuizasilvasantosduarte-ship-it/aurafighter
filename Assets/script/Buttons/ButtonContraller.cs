using UnityEngine;

public class ButtonController : MonoBehaviour
{
    private SpriteRenderer theSR;
    public Sprite defaultImage;
    public Sprite pressedImage;
    public KeyCode keyToPress;

    void Start()
    {
        // Obtém o componente uma única vez [1]
        theSR = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Altera para a imagem de pressionado [2]
        if (Input.GetKeyDown(keyToPress))
        {
            theSR.sprite = pressedImage;
        }

        // Retorna para a imagem padrão ao soltar [2]
        if (Input.GetKeyUp(keyToPress))
        {
            theSR.sprite = defaultImage;
        }
    }

}