using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject storyCanvas;
    public GameObject phonePuzzelCanvas;
    public FlashlightController flashlightController;
    public Hints hints;

    private bool isPaused = false;

    void Update()
    {
        // Проверява дали е натиснат клавиш ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        if(storyCanvas.activeSelf || phonePuzzelCanvas.activeSelf)
        {
            UnlockCursor();
        } else {
           LockCursor();
        
        }
        AudioListener.pause = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        UnlockCursor();
        AudioListener.pause = true;
        hints.HideHint();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
        AudioListener.pause = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
