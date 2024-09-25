using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherMenuButtons : MonoBehaviour
{
    public void ReturnToMenu()
    {
        GameManager.instance.ChangeScene("MainMenuVR");
    }
}
