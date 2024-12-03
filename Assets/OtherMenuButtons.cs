using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherMenuButtons : MonoBehaviour
{
    public void ReturnToMenu()
    {
        DataEvent newEvent2 = new DataEvent("Game Quit", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent2);

        GameManager.instance.ChangeScene("MainMenuXR-V2");
    }
}
