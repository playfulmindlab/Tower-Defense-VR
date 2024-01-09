using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private SaveData playerData;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
    }

    public void SetPlayerData(SaveData newData)
    {
        playerData = newData;
    }

    public SaveData PlayerData()
    {
        return playerData;
    }

    public void IncreaseWaveCount(int addWaves)
    {
        playerData.wave += addWaves;
    }
}
