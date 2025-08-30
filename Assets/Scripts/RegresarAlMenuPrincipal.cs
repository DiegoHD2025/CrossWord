using UnityEngine;
using UnityEngine.SceneManagement;

public class RegresarAlMenuPrincipal : MonoBehaviour
{
    public void Regresar()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
