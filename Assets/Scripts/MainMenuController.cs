using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void GoToMenuDificultad()
    {
        SceneManager.LoadScene("MenuDificultad");
    }

    public void GoToOpciones()
    {
        SceneManager.LoadScene("Opciones");
    }

    public void GoToComoJugar()
    {
        SceneManager.LoadScene("ComoJugar");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
        // Si est�s en el editor de Unity, esto cerrar� el juego cuando hagas clic en Salir.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}

