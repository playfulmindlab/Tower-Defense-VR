using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;


public class SaveDataManager : MonoBehaviour
{
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
            File.Create(dataPath + "/" + fileName + "_hits.csv");
            File.Create(dataPath + "/" + fileName + "_upgrades.csv");
            file = File.Create(dataPath + "/" + fileName + "_save.dat");
        }

        SaveData data = GameManager.instance.PlayerData();

        bf.Serialize(file, data);
        file.Close();

        StreamWriter upgradeWriter = new StreamWriter(dataPath + "/" + fileName + "_upgrades.csv");
        upgradeWriter.WriteLine("Timestamp,Wave,Original,Upgraded To");
        List<UpgradesStruct> upgradeStructs = data.upgradeStructures;

        for (int i = 0; i < upgradeStructs.Count; i++)
        {
            upgradeWriter.WriteLine(upgradeStructs[i].GetUpgradeInfoString());
        }

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
        
    }
}

public struct HitsStruct
{

}

public struct UpgradesStruct
{
    public string timestamp;
    public int wave;
    public string origTower;
    public string upgradedTower;

    public UpgradesStruct(string newTimestamp, int waveNum, string newOrigTower, string newUpgradedTower)
    {
        timestamp = newTimestamp;
        wave = waveNum;
        origTower = newOrigTower;
        upgradedTower = newUpgradedTower;
    }

    public string GetUpgradeInfoString()
    {
        return timestamp.ToString() + "," + wave.ToString() + "," + origTower.ToString() + "," + upgradedTower.ToString();
    }
}
