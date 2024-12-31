using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseControlUI : MonoBehaviour
{
    public static PhaseControlUI instance;

    [SerializeField] Button defendButton;
    [SerializeField] Button cancelButton;
    [SerializeField] Button pauseButton;

    private void Start()
    {
        if (instance == null) instance = this;
        else { Destroy(this); }

        EnableUIButtons();
    }

    public void ToggleDefendButton(bool toggleState)
    {
        defendButton.interactable = toggleState;
    }

    public void EnableUIButtons()
    {
        defendButton.interactable = true;
        cancelButton.interactable = true;
        pauseButton.interactable = true;
    }


    public void DisableUIButtons()
    {
        defendButton.interactable = false;
        cancelButton.interactable = false;
        pauseButton.interactable = false;
    }

    public void ReturnToMenu()
    {
        //TowerDefenseManager.instance.TogglePause();
        //TowerDefenseManager.instance.Reset

        DataEvent newEvent2 = new DataEvent("Game Quit", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent2);

        GameManager.instance.ChangeScene("MainMenuXR-V2");
    }
}
