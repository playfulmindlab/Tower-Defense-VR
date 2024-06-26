using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class ParticipantInputMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField playerIDField;
    [SerializeField] TMP_InputField playerSessionField;
    [SerializeField] TMP_InputField startWorldField;
    [SerializeField] TMP_InputField startPathField;

    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;
    }

    public void InitializeNewParticipant()
    {
        SaveData newSaveData = new SaveData(playerIDField.text, TimeSpan.Zero.ToString(), int.Parse(playerSessionField.text), "DemoImplementScene_HandTracking", 1);
        gm.SetPlayerData(newSaveData);

        gm.LogNewMainMenuEvent("Session Start!");
        gm.ChangeScene("DemoImplementScene_HandTracking");
    }

    public void InitializeNewParticipant_WorldAndPath()
    {
        SaveData newSaveData = new SaveData(playerIDField.text, TimeSpan.Zero.ToString(), int.Parse(playerSessionField.text), startWorldField.text, int.Parse(startPathField.text));
        gm.SetPlayerData(newSaveData);

        if (SceneManager.GetSceneByName(startWorldField.text).IsValid())
        {
            gm.LogNewMainMenuEvent("Session Start!");
            gm.ChangeScene(startWorldField.text);
        }
    }
}
