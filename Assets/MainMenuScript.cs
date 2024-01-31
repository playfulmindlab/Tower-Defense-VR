using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] GameObject optionsCanvas;

    [SerializeField] GameObject quitGamePanel;

    public void NewGame()
    {
        SceneManager.LoadScene("Tower Defense Implementation", LoadSceneMode.Single);
    }

    public void NewGame(string newGameSceneName)
    {
        SceneManager.LoadScene(newGameSceneName, LoadSceneMode.Single);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("LoadGameScreen", LoadSceneMode.Single);
    }

    public void ToggleOptionsMenu(bool toggle)
    {
        optionsCanvas.SetActive(toggle);
    }

    public void QuitGamePanel(bool screenActive)
    {
        quitGamePanel.SetActive(screenActive);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
