using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnablesEnabler : MonoBehaviour
{
    public static SpawnablesEnabler instance;
    [SerializeField] SpawnablesToEnable[] spawnables;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
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
