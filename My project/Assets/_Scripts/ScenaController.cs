using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    // Carga la escena del juego (asegúrate de que se llama exactamente igual en Build Settings)
    public void LoadGameScene(string sceneName)
    {
        Time.timeScale = 1f; // Restaura el tiempo por si venía de pausa
        SceneManager.LoadScene(sceneName);
    }

    // Regresa al menú principal
    public void LoadMenuScene(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    // Cierra el juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}