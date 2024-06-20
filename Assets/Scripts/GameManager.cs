using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private SaveData playerData;
    public SaveData PlayerData{ get { return playerData; } set { } }

    public string currMapName;
    public int currPathNum;
    public int currWaveNum;

    DateTime sessionStartTime;
    public DateTime SessionStartTime { get { return sessionStartTime; } set { } }


    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(this); }
        else if (instance != this) Destroy(this.gameObject);

        sessionStartTime = DateTime.Now;

        playerData = new SaveData("TempPlayer", new TimeSpan(0, 0, 0).ToString(), 1, "DemoImplementScene_HandTracking", 0);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Test Out");

            string saveString = string.Join(" /// ", playerData.ReturnDataAsStrings());
            Debug.Log(saveString);
        }
         
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Test Save");
            SaveDataManager.instance.Save();

            LogNewMainMenuEvent("Save Event");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Test Load");
            SaveDataManager.instance.Load("TempPlayer");

            string loadString = string.Join(" /// ", playerData.ReturnDataAsStrings());
            Debug.Log(loadString);
        }
    }

    public void SetPlayerData(SaveData newData)
    {
        playerData = newData;
    }

    public void LoadNewSaveData(SaveData newSaveData)
    {
        playerData = newSaveData;
    }


    public void ChangeScene(string newSceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(newSceneName, loadMode);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void LogNewEvent(string newEventName, GameObject newGOName, Vector3 newEventCoords, bool isJumped)
    {
        string eventCoordinates = new string("X: " + newEventCoords.x + "Y: " + newEventCoords.y + "Z: " + newEventCoords.z);
        DataEvent newEvent = new DataEvent(newEventName, newGOName.name, eventCoordinates, isJumped.ToString());

        EventManager.instance.RecordNewEvent(newEvent);
    }

    public void LogNewMainMenuEvent(string newEventName) 
    {
        //return new DataEvent(newEventName);
        DataEvent newEvent = new DataEvent(newEventName);

        EventManager.instance.RecordNewEvent(newEvent);
    }
}
