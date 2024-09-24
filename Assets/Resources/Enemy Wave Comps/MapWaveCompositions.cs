using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Wave Comp Data", menuName = "Create Wave Data")]
public class MapWaveCompositions : ScriptableObject
{
    public WaveComposition[] waves;

    public int[][] GetAllWaveEnemyIDs()
    {
        List<int[]> enemyIDHolder = new List<int[]>();

        for (int i = 0; i < waves.Length; i++)
        {
            enemyIDHolder.Add(waves[i].GetEnemyIDs_Looped());
        }

        return enemyIDHolder.ToArray();
    }

    public int[] GetAllEnemiesInEachWave()
    {
        List<int> enemyCountHolder = new List<int>();

        for (int i = 0; i < waves.Length; i++)
        {
            enemyCountHolder.Add(waves[i].enemyOrder.Length * waves[i].numLoops);
        }

        return enemyCountHolder.ToArray();
    }

    public PostWaveStep[] MapPostWaveSteps()
    {
        List<PostWaveStep> newPWSHolder = new List<PostWaveStep>();

        for (int i = 0; i < waves.Length; i++)
        {
            newPWSHolder.Add(waves[i].postWaveStep);
        }

        return newPWSHolder.ToArray();
    }
}

[System.Serializable]
public struct WaveComposition
{
    public EnemySummonData[] enemyOrder;
    public int numLoops;
    public PostWaveStep postWaveStep;

    public int[] GetEnemyIDs_Looped()
    {
        List<int> partialIDHolder = new List<int>();
        List<int> fullIDHolder = new List<int>();

        for (int i = 0; i < enemyOrder.Length; i++)
        {
            partialIDHolder.Add(enemyOrder[i].enemyID);
        }

        for (int j = 0; j < numLoops; j++)
        {
            fullIDHolder.AddRange(partialIDHolder);
        }

        return fullIDHolder.ToArray();
    }
}

[SerializeField]
public enum PostWaveStep { Continue, Intermission, LevelUp };
