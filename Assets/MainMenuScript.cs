using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] GameObject quitGamePanel;

    public void NewGame()
    {
        SceneManager.LoadScene("Tower Defense Implementation", LoadSceneMode.Single);
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("LoadGameScreen", LoadSceneMode.Single);
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
