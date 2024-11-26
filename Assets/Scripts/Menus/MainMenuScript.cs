using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    [SerializeField] GameObject newGamePanel;
    [SerializeField] GameObject fileSelectPanel;
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject quitGamePanel;

    [SerializeField] Canvas mainMenuCanvas;
    [SerializeField] Canvas settingsCanvas;
    [SerializeField] Canvas tutorialCanvas;

    public void NewGame()
    {
        GameManager.instance.ChangeScene("DemoImplementScene_HandTracking");
    }

    public void LoadGame()
    {
        GameManager.instance.ChangeScene("LoadGameScreen");
    }

    public void ToggleSettingsMenu(bool toggle)
    {
        settingsCanvas.enabled = toggle;
        mainMenuCanvas.enabled = !toggle;
    }

    public void ToggleTutorial(bool toggle)
    {
        tutorialCanvas.enabled = toggle;
        mainMenuCanvas.enabled = !toggle;
    }

    public void QuitGamePanel(bool screenActive)
    {
        quitGamePanel.SetActive(screenActive);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OpenCurrentPanel(GameObject panelToOpen)
    {
        panelToOpen.SetActive(true);
    }

    public void CloseCurrentPanel(GameObject openPanel)
    {
        openPanel.SetActive(false);
    }
}
