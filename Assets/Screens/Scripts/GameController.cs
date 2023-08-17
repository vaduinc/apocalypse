using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    public void PlayGame()
    {
        SceneManager.LoadScene("TerreainScene");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        // Quit the Editor Play mode if in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Quit the application if in a standalone build
        Application.Quit();
#endif
    }

}
