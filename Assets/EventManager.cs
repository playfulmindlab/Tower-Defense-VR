using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class EventManager : MonoBehaviour//Singleton<Analytics>
{
    public static EventManager instance;

    string fileLocation = "TestEvents";


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this.gameObject);

        //SwitchFileLocations("Main Menu Events");
    }

    void Start()
    {
        SwitchFileLocations("Main Menu Events");
    }

    public void SwitchFileLocations(string newLocation = "N/A")
    {
        string newFileLocation = "";

        if (newLocation == "N/A")
        {
            SaveData playerData = GameManager.instance.PlayerData;
            newFileLocation = Application.dataPath + "/Events/" + playerData.PlayerID + "/";
        }
        else
        {
            newFileLocation = Application.dataPath + "/Events/" + newLocation + "/";
        }

        if (!File.Exists(newFileLocation))
        {
            Directory.CreateDirectory(newFileLocation);
        }

        newFileLocation += GameManager.instance.SessionStartTime.ToString("dd-MM-yy - HH.mm.ss") + ".csv";
        File.WriteAllText(newFileLocation, "Timestamp,Participant,Game,Total Time Played,Session Number,Session Time Played,Map Number,Level Number,Wave Number,Event,Gameobject,Gameobject Coordinates,Jump Status, Enemy Health\n");
        fileLocation = newFileLocation;

        Debug.Log("Swapped Files.");
    }

    public void RecordNewEvent(DataEvent newEvent)
    {
        Debug.Log("Recording Event...");
        string newEventString = string.Join(",", newEvent.GetDataStringArray());

        StreamWriter writer;
        writer = new StreamWriter(fileLocation, true);

        writer.WriteLine(newEventString);

        writer.Close();

        Debug.Log("Event Recorded! - " + newEvent.eventName);
    }
    
}



[System.Serializable]
public class DataEvent
{
    public string timestamp;
    public string participantID;
    public string gameName;
    public string totalTimePlayed;
    public string sessionNumber;
    public string currentSessionTime;
    public string mapNumber;
    public string levelNumber;
    public string waveNumber;
    public string eventName;
    public string gameObjectName;
    public string eventCoordinates;
    public string jumpStatus;

    public string enemyHealth = "N/A";

    public DataEvent(string newEventName, GameObject newGO, Vector3 newEventCoords, bool isJumped)
    {
        GameManager gm = GameManager.instance;
        SaveData playerData = gm.PlayerData;

        //most of the data & functions are stored in either the GameManager or the SaveData class (in GameManager > SaveData PlayerData)
        timestamp = DateTime.Now.ToString("HH.mm.ss.fff");
        participantID = playerData.PlayerID;
        gameName = Application.productName;
        totalTimePlayed = playerData.GetTotalTimePlayed();
        sessionNumber = playerData.TimesFileOpened.ToString();
        currentSessionTime = playerData.GetCurrentSessionPlaytime();

        mapNumber = gm.currMapName;
        levelNumber = gm.currPathNum.ToString();
        waveNumber = gm.currWaveNum.ToString();

        eventName = newEventName;
        gameObjectName = newGO.name;
        eventCoordinates = new string ("X: " + newEventCoords.x + " Y: " + newEventCoords.y + " Z: " + newEventCoords.z);
        jumpStatus = isJumped.ToString();
    }

    public DataEvent(string newEventName, string newGOName, string newEventCoords, string isJumped)
    {
        GameManager gm = GameManager.instance;
        SaveData playerData = gm.PlayerData;

        //most of the data & functions are stored in either the GameManager or the SaveData class (in GameManager > SaveData PlayerData)
        timestamp = DateTime.Now.ToString("HH.mm.ss.fff");
        participantID = playerData.PlayerID;
        gameName = Application.productName;
        totalTimePlayed = playerData.GetTotalTimePlayed();
        sessionNumber = playerData.TimesFileOpened.ToString();
        currentSessionTime = playerData.GetCurrentSessionPlaytime();

        mapNumber = gm.currMapName;
        levelNumber = gm.currPathNum.ToString();
        waveNumber = gm.currWaveNum.ToString();

        eventName = newEventName;
        gameObjectName = newGOName;
        eventCoordinates = newEventCoords;
        jumpStatus = isJumped;
    }

    public DataEvent(string newEventName)
    {
        GameManager gm = GameManager.instance;
        SaveData playerData = gm.PlayerData;

        //most of the data & functions are stored in either the GameManager or the Player File (at GameManager > SaveData PlayerData)
        timestamp = DateTime.Now.ToString("HH.mm.ss.fff");
        gameName = Application.productName;

        if (playerData != null)
        {
            participantID = playerData.PlayerID;
            totalTimePlayed = playerData.GetTotalTimePlayed();
            sessionNumber = playerData.TimesFileOpened.ToString();
            currentSessionTime = playerData.GetCurrentSessionPlaytime();
        }
        else
        {
            participantID = "MainMenuScene";
            totalTimePlayed = "N/A";
            sessionNumber = "N/A";
            currentSessionTime = "N/A";
        }

        mapNumber = "N/A";
        levelNumber = "N/A";
        waveNumber = "N/A";

        eventName = newEventName;
        gameObjectName = "N/A";
        eventCoordinates = "N/A";
        jumpStatus = "N/A";
    }

    public string[] GetDataStringArray()
    {
        List<string> newEventStringList = new List<string>();

        newEventStringList.Add(timestamp);
        newEventStringList.Add(participantID);
        newEventStringList.Add(gameName);
        newEventStringList.Add(totalTimePlayed);
        newEventStringList.Add(sessionNumber);
        newEventStringList.Add(currentSessionTime);
        newEventStringList.Add(mapNumber);
        newEventStringList.Add(levelNumber);
        newEventStringList.Add(waveNumber);
        newEventStringList.Add(eventName);
        newEventStringList.Add(gameObjectName);
        newEventStringList.Add(eventCoordinates);
        newEventStringList.Add(jumpStatus);

        newEventStringList.Add(enemyHealth != "N/A" ? enemyHealth : "");

        return newEventStringList.ToArray();
    }
}

