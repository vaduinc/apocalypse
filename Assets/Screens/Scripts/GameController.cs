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
        Application.Quit();
    }

}
