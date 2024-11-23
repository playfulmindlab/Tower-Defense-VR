using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EnemySpawner : MonoBehaviour
{
    public static List<Enemy> enemiesInGame;
    public static List<Transform> enemiesInGameTransform;

    public static Dictionary<Transform, Enemy> enemyTransformPairs;
    public static Dictionary<int, GameObject> enemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> enemyObjectPools;

    private static bool isInitialized;

    [SerializeField] MapWaveCompositions currMapWaveComp;
    [SerializeField] private static int[][] enemyNumbers;
    private static int[][] waveAndEnemyIDOrder; // [wave][currentEnemyID]
    public static int[] numEnemiesInWaves;
    public static PostWaveStep[] postWaveSteps;

    static int currEnemySpawned = 0;
    public static int totalWaves = 5;

    public void Start()
    {
        //enemyNumbers = enemyIDHolder.ToArray();
        //numEnemiesInWaves = waveCountHolder.ToArray();
        //afterWaveStatus = afterStatusHolder.ToArray();

        enemyNumbers = currMapWaveComp.GetAllWaveEnemyIDs();
        numEnemiesInWaves = currMapWaveComp.GetAllEnemiesInEachWave();
        postWaveSteps = currMapWaveComp.MapPostWaveSteps();

        Debug.Log("INITIALIZING ENEMY LIST...");
        List<int[]> newOrder = new List<int[]>();
        Debug.Log("ENEMY LENGTH: " + enemyNumbers.Length);
        for (int i = 0; i < enemyNumbers.Length; i++)
        {
            newOrder.Add(enemyNumbers[i]);
        }
        waveAndEnemyIDOrder = newOrder.ToArray();
        totalWaves = waveAndEnemyIDOrder.Length;
        
        string debugString = "";
        for (int z = 0; z < waveAndEnemyIDOrder.Length; z++)
        {
            foreach (var x in waveAndEnemyIDOrder[z])
            {
                debugString += x.ToString() + " > ";
            }
            debugString += waveAndEnemyIDOrder[z].Length;

            Debug.Log("WAVE #" + z + " ORDER: " + debugString + " // TOTAL: " + numEnemiesInWaves[z] + " NEXT: " + postWaveSteps[z]);///afterWaveStatus[z]);
            debugString = "";
        }
        
    }

    public static void Init()
    {
        if (!isInitialized)
        {
            enemiesInGame = new List<Enemy>();
            enemiesInGameTransform = new List<Transform>();
            enemyTransformPairs = new Dictionary<Transform, Enemy>();
            enemyPrefabs = new Dictionary<int, GameObject>();
            enemyObjectPools = new Dictionary<int, Queue<Enemy>>();

            EnemySummonData[] enemyData = Resources.LoadAll<EnemySummonData>("EnemyData");

            foreach (EnemySummonData enemy in enemyData)
            {
                enemyPrefabs.Add(enemy.enemyID, enemy.enemyPrefab);
                enemyObjectPools.Add(enemy.enemyID, new Queue<Enemy>());
            }

            isInitialized = true;
        }
        else
        {
            Debug.Log("EnemySpawner is already initialized!");

            enemiesInGame.Clear();
            enemiesInGameTransform.Clear();
            enemyTransformPairs.Clear();
            enemyPrefabs.Clear();
            enemyObjectPools.Clear();

            EnemySummonData[] enemyData = Resources.LoadAll<EnemySummonData>("EnemyData");

            foreach (EnemySummonData enemy in enemyData)
            {
                enemyPrefabs.Add(enemy.enemyID, enemy.enemyPrefab);
                enemyObjectPools.Add(enemy.enemyID, new Queue<Enemy>());
            }
        }
    }

    public static Enemy SummonEnemy(int newEnemyID)
    {
        Enemy newEnemy = null;

        if (!enemyPrefabs.ContainsKey(newEnemyID))
        {
            Debug.Log("There is no enemy with ID " + newEnemyID + "!");
            return null;
        }

        Queue<Enemy> referencedQueue = enemyObjectPools[newEnemyID];

        if (referencedQueue.Count > 0)
        {
            //dequeue enemy & initialize
            newEnemy = referencedQueue.Dequeue();
            newEnemy.Init();

            newEnemy.gameObject.SetActive(true);
        }
        else
        {
            //instantiate new instance of enemy & initialize
            GameObject newEnemyObject = Instantiate(enemyPrefabs[newEnemyID], TowerDefenseManager.nodePositions[0], Quaternion.identity);
            newEnemy = newEnemyObject.GetComponent<Enemy>();
            newEnemy.Init();
        }

        if (!enemiesInGame.Contains(newEnemy)) enemiesInGame.Add(newEnemy);
        if (!enemiesInGameTransform.Contains(newEnemy.transform)) enemiesInGameTransform.Add(newEnemy.transform);
        if (!enemyTransformPairs.ContainsKey(newEnemy.transform)) enemyTransformPairs.Add(newEnemy.transform, newEnemy);

        newEnemy.id = newEnemyID;

        return newEnemy;
    }

    public static int GetNextIDToSpawn()
    {
        int currWave = TowerDefenseManager.waveCount - 1;

        int newID = waveAndEnemyIDOrder[currWave][currEnemySpawned];
        currEnemySpawned++;

        if (currEnemySpawned >= waveAndEnemyIDOrder[currWave].Length)
        {
            currEnemySpawned = 0;
        }
        return newID;     
    }

    public static void RemoveEnemy(Enemy enemyToRemove)
    {
        enemyObjectPools[enemyToRemove.id].Enqueue(enemyToRemove);
        enemyToRemove.gameObject.SetActive(false);
        enemyToRemove.ChangeTowerTarget(null);
        enemiesInGameTransform.Remove(enemyToRemove.transform);
        enemyTransformPairs.Remove(enemyToRemove.transform);

        enemiesInGame.Remove(enemyToRemove);
    }
}
