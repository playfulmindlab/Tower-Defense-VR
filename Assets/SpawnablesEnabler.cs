using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnablesEnabler : MonoBehaviour
{
    public static SpawnablesEnabler instance;

    [SerializeField] bool activateUnlockableTowers;
    [SerializeField] SpawnablesToEnable[] spawnables;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        PropSpawnableScript[] allpsss = GetComponentsInChildren<PropSpawnableScript>();
        foreach (PropSpawnableScript pss in allpsss)
        {
            if (activateUnlockableTowers == true)
            {
                pss.DisableSpawnable();
                pss.ToggleLockedStat(true);
            }
            else
            {
                pss.EnableSpawnable();
                pss.ToggleLockedStat(false);
            }
        }
    }

    public void WaveUpdate(int newWave, int currMoney)
    {
        foreach (SpawnablesToEnable spawnable in spawnables)
        {
            if (spawnable.wave <= newWave)
            {
                spawnable.EnableSpawnables(currMoney);
                //spawnable.ToggleLockedStat(true);
            }
        }
    }

    public void DisableAllTowers()
    {
        if (activateUnlockableTowers == true)
        {
            PropSpawnableScript[] allpsss = GetComponentsInChildren<PropSpawnableScript>();
            foreach (PropSpawnableScript pss in allpsss)
            {
                pss.DisableSpawnable();
                pss.ToggleLockedStat(true);
            }
        }
    }

    public void UpdateUnaffordableTowers(int playerMoney)
    {
        PropSpawnableScript[] allpsss = GetComponentsInChildren<PropSpawnableScript>();
        foreach (PropSpawnableScript pss in allpsss)
        {
            if (pss.TowerCost > playerMoney)
            {
                pss.DisableSpawnable();
            }
            else if (pss.TowerCost <= playerMoney && !pss.isLocked)
            {
                pss.EnableSpawnable();
            }
        }
    }
    
}

[System.Serializable]
public struct SpawnablesToEnable
{
    public int wave;
    public PropSpawnableScript[] spawnables;

    public SpawnablesToEnable(int newWave, PropSpawnableScript[] newSpawnables)
    {
        wave = newWave;
        spawnables = newSpawnables;
    }

    public void EnableSpawnables(int currMoney)
    {
        foreach(PropSpawnableScript s in spawnables)
        {
            if (s.TowerCost < currMoney)
                s.EnableSpawnable();
        }
    }
}
