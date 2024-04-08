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
        GameManager.instance.ChangeScene("DemoImplementScene");
    }

    public void LoadGame()
    {
        GameManager.instance.ChangeScene("LoadGameScreen");
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
