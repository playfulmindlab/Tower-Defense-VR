using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static List<Enemy> enemiesInGame;
    public static List<Transform> enemiesInGameTransform;

    public static Dictionary<Transform, Enemy> enemyTransformPairs;
    public static Dictionary<int, GameObject> enemyPrefabs;
    public static Dictionary<int, Queue<Enemy>> enemyObjectPools;

    private static bool isInitialized;

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

    public static void RemoveEnemy(Enemy enemyToRemove)
    {
        enemyObjectPools[enemyToRemove.id].Enqueue(enemyToRemove);
        enemyToRemove.gameObject.SetActive(false);
        enemiesInGameTransform.Remove(enemyToRemove.transform);
        enemyTransformPairs.Remove(enemyToRemove.transform);

        enemiesInGame.Remove(enemyToRemove);
    }
}
