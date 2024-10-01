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
        BarrelSpawnableScript[] allBSSs = GetComponentsInChildren<BarrelSpawnableScript>();
        foreach (BarrelSpawnableScript bss in allBSSs)
        {
            if (activateUnlockableTowers == true)
            {
                bss.DisableSpawnable();
                bss.ToggleLockedStat(true);
            }
            else
            {
                bss.EnableSpawnable();
                bss.ToggleLockedStat(false);
            }
        }
    }

    public void WaveUpdate(int newWave)
    {
        foreach (SpawnablesToEnable spawnable in spawnables)
        {
            if (spawnable.wave == newWave)
            {
                spawnable.EnableSpawnables();
            }
        }
    }

    public void UpdateUnaffordableTowers(int playerMoney)
    {
        BarrelSpawnableScript[] allBSSs = GetComponentsInChildren<BarrelSpawnableScript>();
        foreach (BarrelSpawnableScript bss in allBSSs)
        {
            if (bss.TowerCost > playerMoney)
            {
                bss.DisableSpawnable();
            }
            else if (bss.TowerCost <= playerMoney && !bss.isLocked)
            {
                bss.EnableSpawnable();
            }
        }
    }
    
}

[System.Serializable]
public struct SpawnablesToEnable
{
    public int wave;
    public BarrelSpawnableScript[] spawnables;

    public SpawnablesToEnable(int newWave, BarrelSpawnableScript[] newSpawnables)
    {
        wave = newWave;
        spawnables = newSpawnables;
    }

    public void EnableSpawnables()
    {
        foreach(BarrelSpawnableScript s in spawnables)
        {
            s.EnableSpawnable();
        }
    }
}
