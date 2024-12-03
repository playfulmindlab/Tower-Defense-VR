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
    }

    public void ToggleDefendButton(bool toggleState)
    {
        defendButton.interactable = toggleState;
    }

    public void DisableUIButtons()
    {
        defendButton.interactable = false;
        cancelButton.interactable = false;
        pauseButton.interactable = false;
    }

    public void ReturnToMenu()
    {
        DataEvent newEvent2 = new DataEvent("Game Quit", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent2);
        TowerDefenseManager.instance.TogglePause();

        GameManager.instance.ChangeScene("MainMenuXR-V2");
    }
}
