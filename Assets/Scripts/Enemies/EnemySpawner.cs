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

    [SerializeField] string enemyNumbersLoc;
    [SerializeField] private static int[][] enemyNumbers; //new order is Lobber, Saboteur, Standard, Tank
    private static int[][] waveAndEnemyIDOrder;//[wave][currentEnemyID]
    public static int[] numEnemiesInWaves;
    public static char[] afterWaveStatus;
    //static int randEnemyLoopCap = 0;

    static int currEnemySpawned = 0;
    public static int totalWaves = 5;

    public void Start()
    {
        List<int[]> numHolder = new List<int[]>();
        List<int> waveCountHolder = new List<int>();
        int enemyCount = 0;

        List<char> afterStatusHolder = new List<char>();

        StreamReader reader = new StreamReader("Assets/Resources/Enemy Numbers/" + enemyNumbersLoc + ".csv");
        bool endOfFile = false;

        reader.ReadLine(); //skip the first line with column labels

        while (!endOfFile)
        {
            string dataString = reader.ReadLine();

            if (dataString == null)
            {
                endOfFile = true;
                break;
            }

            string[] dataValues = dataString.Split(',');

            string[] tempData = new string[5];
            System.Array.Copy(dataValues, tempData, 5);
            int[] enemyValues = System.Array.ConvertAll(tempData, int.Parse);

            enemyCount = 0;
            for (int i = 0; i < 4; i++)
            {
                enemyCount += enemyValues[i];
            }

            enemyCount *= enemyValues[4]; //enemyValues[4] = loop count

            numHolder.Add(enemyValues);
            waveCountHolder.Add(enemyCount);
            afterStatusHolder.Add(System.Convert.ToChar(dataValues[dataValues.Length - 1]));
        }

        reader.Close();

        enemyNumbers = numHolder.ToArray();
        numEnemiesInWaves = waveCountHolder.ToArray();
        afterWaveStatus = afterStatusHolder.ToArray();

        //Debug.Log("ENEMIES: " + enemyNumbers[12][3]);

        Debug.Log("INITIALIZING ENEMY LIST...");
        List<int[]> newOrder = new List<int[]>();
        for (int i = 0; i < enemyNumbers.Length; i++)
        {
            newOrder.Add(GetWaveSpawnOrder(enemyNumbers[i]));
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

            Debug.Log("WAVE #" + z + " ORDER: " + debugString + " // TOTAL: " + numEnemiesInWaves[z] + " NEXT: " + afterWaveStatus[z]);
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

    static int[] GetWaveSpawnOrder(int[] waveEnemyNums)
    {
        if (waveEnemyNums.Length != 5)
            return null;

        List<int> newEnemyOrder = new List<int>();

        if (waveEnemyNums[4] == 1)
        {
            for (int id = 0; id < waveEnemyNums.Length - 1; id++)
            {
                for (int numToSpawn = 0; numToSpawn < waveEnemyNums[id]; numToSpawn++)
                {
                    newEnemyOrder.Add(id);
                }
            }
        }
        else
        {
            for (int h = 0; h < waveEnemyNums[4]; h++)
            {
                for (int id = 0; id < waveEnemyNums.Length - 1; id++)
                {
                    for (int numToSpawn = 0; numToSpawn < waveEnemyNums[id]; numToSpawn++)
                    {
                        newEnemyOrder.Add(id);
                    }
                }
            }
        }

        return newEnemyOrder.ToArray();
    }

    public static int GetNextIDToSpawn()
    {
        int currWave = TowerDefenseManager.waveCount - 1;

        //if (currEnemySpawned < waveAndEnemyIDOrder[currWave].Length)
        //{
            int newID = waveAndEnemyIDOrder[currWave][currEnemySpawned];
            currEnemySpawned++;
        //}

        if (currEnemySpawned >= waveAndEnemyIDOrder[currWave].Length)
        {
            currEnemySpawned = 0;
        }
        return newID;     
    }

    /*
    //uncomment this section to get random enemies spawn order
    public static int GetValidIDToSpawn()
    {
        int currWave = TowerDefenseManager.waveCount - 1;
        int newEnemyID = Random.Range(0, 4);

        //Debug.Log("SafetyTest: " + currWave + "/" + newEnemyID + " = " + enemyNumbers[currWave][newEnemyID]);

        if (randEnemyLoopCap >= 12)
        {
            Debug.LogError("Too many tries! Spawning Standard Enemy as Default!");
            randEnemyLoopCap = 0;
            return 0;
        }

        if (enemyNumbers[currWave][newEnemyID] >= 1)
        {
            enemyNumbers[currWave][newEnemyID]--;
            Debug.Log("Enemy ID " + newEnemyID + " is OK! Can spawn " + enemyNumbers[currWave][newEnemyID] + " more enemies with ID " + newEnemyID + " // Attempt #" + randEnemyLoopCap);
        }
        else
        {
            randEnemyLoopCap++;
            Debug.Log("Can't spawn any more enemies with ID " + newEnemyID + " - Trying Again! Attempt #" + randEnemyLoopCap);

            return GetValidIDToSpawn();
        }

        randEnemyLoopCap = 0;

        return newEnemyID;
    }
    */

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
