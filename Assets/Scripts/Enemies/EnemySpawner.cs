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
    [SerializeField] private static int[][] enemyNumbers; //Order is Standard, Tank, Lobber, Saboteur
    public static int[] numEnemiesInWaves;
    static int randEnemyLoopCap = 0;

    public void Start()
    {
        List<int[]> numHolder = new List<int[]>();
        List<int> waveCountHolder = new List<int>();
        int enemyCount = 0;

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

            int[] dataValues = System.Array.ConvertAll(dataString.Split(','), int.Parse);

            numHolder.Add(dataValues);

            enemyCount = 0;
            foreach (int num in dataValues)
            {
                enemyCount += num;
                //Debug.Log("ENEMY COUNTING: " + enemyCount);
            }

            waveCountHolder.Add(enemyCount);
        }

        reader.Close();

        enemyNumbers = numHolder.ToArray();
        numEnemiesInWaves = waveCountHolder.ToArray();

        //Debug.Log("ENEMIES: " + enemyNumbers[12][3]);
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
            //Debug.Log(enemyData[0].name);

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

    public static int GetValidIDToSpawn()
    {
        int currWave = TowerDefenseManager.waveCount - 1;
        int newEnemyID = Random.Range(0, 4);

        Debug.Log("SafetyTest: " + currWave + "/" + newEnemyID + " = " + enemyNumbers[currWave][newEnemyID]);

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

    /*public static Enemy SummonRandomEnemy()
    {
        int currWave = GameManager.instance.currWaveNum;
        int newEnemyID = Random.Range(0, 4);

        Enemy newEnemy = null;
        Debug.Log("SafetyTest: " + currWave + "/" + newEnemyID + " = " + enemyNumbers[currWave][newEnemyID]);

        if (!enemyPrefabs.ContainsKey(newEnemyID))
        {
            Debug.Log("There is no enemy with ID " + newEnemyID + "!");
            return null;
        }

        if (randEnemyLoopCap >= 20)
        {
            Debug.LogError("Too many tries! Enemy will not be spawned!");
            randEnemyLoopCap = 0;
            return null;
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

            SummonRandomEnemy();
            return null;
        }

        randEnemyLoopCap = 0;



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
    }*/

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
