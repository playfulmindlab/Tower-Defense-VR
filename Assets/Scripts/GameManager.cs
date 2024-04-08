using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void ChangeScene(string newSceneName, LoadSceneMode loadMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(newSceneName, loadMode);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
