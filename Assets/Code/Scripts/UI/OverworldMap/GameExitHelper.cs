using UnityEngine;
using UnityEngine.SceneManagement;

public class GameExitHandler : MonoBehaviour
{
    // Quits the game
    public void QuitGame()
    {
#if UNITY_EDITOR
        // If running in the Unity Editor.
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // If running as a built application
            Application.Quit();
#endif
    }

    // Loads the start screen scene.
    public void LoadStartScreen()
    {
        DestroyCurrentGameManager();
        SceneManager.LoadScene("start_screen", LoadSceneMode.Single);
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
    }

    private void DestroyCurrentGameManager()
    {
        GameObject gameManagerInstance = GameObject.FindGameObjectWithTag("GameManager");
        Destroy(gameManagerInstance);
    }
}