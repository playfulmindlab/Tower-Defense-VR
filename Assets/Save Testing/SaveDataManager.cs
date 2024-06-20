using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;


public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager instance;


    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(this); }
        else if (instance != this) Destroy(this.gameObject);

        // Update the path once the persistent path exists.
        //saveFile = Application.persistentDataPath + "/gamedata.json";
    }

    


    public void Save()
    {
        SaveData newSaveData = GameManager.instance.PlayerData;

        string[] saveDataContent = newSaveData.ReturnDataAsStrings();

        string saveString = string.Join(" ## ", saveDataContent);

        Debug.Log("Save Location: " + Application.dataPath + "/" + newSaveData.PlayerID + ".txt");
        File.WriteAllText(Application.dataPath + "/" + newSaveData.PlayerID + ".txt", saveString);

        Debug.Log("Saved!");
    }

    public void Load(string loadFileName)
    {
        if (File.Exists(Application.dataPath + "/" + loadFileName + ".txt"))
        {
            string saveString = File.ReadAllText(Application.dataPath + "/" + loadFileName + ".txt");

            string[] loadedGameContent = saveString.Split(" ## ");

            SaveData loadedGame = new SaveData(loadedGameContent);

            if (loadedGame != null)
            {
                GameManager.instance.LoadNewSaveData(loadedGame);
                Debug.Log("Loaded!");
            }
            else
            {
                Debug.LogError("ERROR: Could not load save file!");
            }
        }
        else
        {
            Debug.LogError("ERROR: Could not find save file " + loadFileName);
        }
    }




    /*



    public void SetCurrentSaveFile(string fileName)
    {
        PlayerPrefs.SetString("CurrentFile", fileName);
    }

    public void CreateNewSaveFile()
    {
        int saveFileNum = PlayerPrefs.GetInt("SaveFileCount") + 1;

        PlayerPrefs.SetInt("SaveFileCount", saveFileNum);
    }

    public void SaveGame()
    {
        PlayerPrefs.SetString("CurrentFile", "testSaveFile");

        string fileName = PlayerPrefs.GetString("CurrentFile");
        string dataPath = Application.persistentDataPath;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(dataPath))
        {
            file = File.Open(dataPath + "/" + fileName + "_save.dat", FileMode.Open);
        }
        else
        {
            Debug.Log(dataPath);
            file = File.Create(dataPath + "/" + fileName + "_save.dat");
        }

        SaveData data = GameManager.instance.PlayerData;

        bf.Serialize(file, data);
        file.Close();

        StreamWriter upgradeWriter = new StreamWriter(dataPath + "/" + fileName + "_upgrades.csv");
        upgradeWriter.WriteLine("Timestamp,Wave,Original,Upgraded To");
        //List<UpgradesStruct> upgradeStructs = data.upgradeStructures;

        //for (int i = 0; i < upgradeStructs.Count; i++)
        //{
        //    upgradeWriter.WriteLine(upgradeStructs[i].GetUpgradeInfoString());
        //}

        upgradeWriter.Close();
    }

    public void LoadGame()
    {
        string fileName = PlayerPrefs.GetString("CurrentFile");
        string dataPath = Application.persistentDataPath;

        if (File.Exists(dataPath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(dataPath + "/" + fileName + "_save.dat", FileMode.Open);
            SaveData data = (SaveData)bf.Deserialize(file);

            GameManager.instance.SetPlayerData(data);

            file.Close();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.SetString("CurrentFile", "testSaveFile");
        SaveGame();
        //PlayerPrefs.SetInt("SaveFileCount", 0);
        //Debug.Log("File: " + PlayerPrefs.GetInt("SaveFileCount"));
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}

public class SaveData
{
    string fileID;
    string totalPlayTime;
    int totalTimesThisFilePlayed;
    string mapName;
    int pathNum;

    public string PlayerID { get { return fileID; } set { } }
    public int TimesFileOpened { get { return totalTimesThisFilePlayed; } set { } }

    public SaveData(string newFileID, string newTotalPlayTime, int newNumTimesPlayed, string newMapName, int newPathNum)
    {
        fileID = newFileID;
        totalPlayTime = newTotalPlayTime;
        totalTimesThisFilePlayed = newNumTimesPlayed;
        mapName = newMapName;
        pathNum = newPathNum;
    }

    public SaveData(string[] newStringParams)
    {
        if (newStringParams.Length == 5)
        {
            fileID = newStringParams[0];
            totalPlayTime = newStringParams[1];
            totalTimesThisFilePlayed = int.Parse(newStringParams[2]);
            mapName = newStringParams[3];
            pathNum = int.Parse(newStringParams[4]);
        }
        else
        {
            Debug.LogError("File could not be loaded - detected " + newStringParams.Length + " parameters!");
        }

    }

    public void OpenedSaveFile()
    {
        totalTimesThisFilePlayed++;
    }

    public string[] ReturnDataAsStrings()
    {
        string[] saveDataStringArray = new string[]
        {
            fileID,
            totalPlayTime,
            totalTimesThisFilePlayed.ToString(),
            mapName,
            pathNum.ToString()
        };

        return saveDataStringArray;
    }

    public void UpdateSaveData(DateTime startGameTime, string currMapName, int currPathNum)
    {
        UpdatePlayTime(startGameTime);
        mapName = currMapName;
        pathNum = currPathNum;
    }

    public string GetCurrentSessionPlaytime()
    {
        TimeSpan currSessionTime = DateTime.Now - GameManager.instance.SessionStartTime;
        return currSessionTime.ToString();
    }

    public string GetTotalTimePlayed()
    {
        return UpdatePlayTime(GameManager.instance.SessionStartTime);
    }

    string UpdatePlayTime(DateTime gameStartTime)
    {
        TimeSpan currSessionTime = DateTime.Now - gameStartTime;
        TimeSpan totalPlayTimeSpan = TimeSpan.Parse(totalPlayTime);
        totalPlayTimeSpan += currSessionTime;
        return totalPlayTimeSpan.ToString();
    }
}

