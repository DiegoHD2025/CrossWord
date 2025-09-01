using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuDificultadController : MonoBehaviour
{
    public void GoToBasicGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void GoToIntermediateGame()
    {
        SceneManager.LoadScene("IntermediateGame");
    }

    public void GoToAdvancedGame()
    {
        SceneManager.LoadScene("AdvanceGame");
    }

    public void RegresarAlMenuPrincipal()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
