using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void StartGame()
    {
        SceneManager.LoadScene("LoadingScreenHints");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
