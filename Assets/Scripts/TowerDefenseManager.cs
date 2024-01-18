using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;

public class TowerDefenseManager : MonoBehaviour
{
    public static List<TowerBehaviour> towersInGame;
    public static Vector3[] nodePositions;
    public static float[] nodeDistances;
    public static int waveCount = 1;

    private static Queue<int> enemyIDsToSpawnQueue;
    private static Queue<Enemy> enemiesToRemoveQueue;
    private static Queue<EnemyDamage> damageData;
    private static Queue<AppliedEffect> effectsQueue;

    private PlayerStats playerStats;

    public Transform nodeParent;
    public bool continueLoop = true;
    public bool spawnEnemies = true;

    [SerializeField] GameObject colliderObject;
    [SerializeField] int enemyKillCount;
    [SerializeField] int currEnemyKillCount;
    int spawnedEnemiesCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        towersInGame = new List<TowerBehaviour>();
        enemyIDsToSpawnQueue = new Queue<int>();
        enemiesToRemoveQueue = new Queue<Enemy>();
        damageData = new Queue<EnemyDamage>();
        effectsQueue = new Queue<AppliedEffect>();
        playerStats = FindObjectOfType<PlayerStats>();
        EnemySpawner.Init();

        nodePositions = new Vector3[nodeParent.childCount];
        for (int i = 0; i < nodePositions.Length; i++)
        {
            nodePositions[i] = nodeParent.GetChild(i).position;
        }

        nodeDistances = new float[nodePositions.Length - 1];
        for (int i = 0; i < nodeDistances.Length; i++)
        {
            nodeDistances[i] = Vector3.Distance(nodePositions[i], nodePositions[i + 1]);
        }

        UpdateWaveCount(1);

        StartCoroutine(GameplayLoop());

        //InvokeRepeating("SpawnTest", 0f, 1f);
        //InvokeRepeating("RemoveTest", 0f, 2f);
    }

    void SpawnTest() { EnqueueEnemyIDToSummon(1); }

    void RemoveTest()
    {
        if (EnemySpawner.enemiesInGame.Count > 0)
        {
            EnemySpawner.RemoveEnemy(EnemySpawner.enemiesInGame[Random.Range(0, EnemySpawner.enemiesInGame.Count)]);
        }
    }

    public void UpdateWaveCount(int newWaveNum = -1)
    {
        if (newWaveNum < 0)
            waveCount++;

        //CancelInvoke();

        enemyKillCount = (waveCount * 5);
        currEnemyKillCount = 0;
        spawnedEnemiesCount = 0;

        playerStats.DisplayWaveCount(waveCount);
        playerStats.DisplayEnemyCount(enemyKillCount);

        spawnEnemies = true;
        InvokeRepeating("SpawnTest", 0f, 1f);
    }

    public void UpdateEnemyCount()
    {
        currEnemyKillCount++;
        playerStats.DisplayEnemyCount(enemyKillCount - currEnemyKillCount);
    }

    IEnumerator GameplayLoop()
    {
        while(continueLoop == true)
        {
            //Debug.Log("QUEUE COUNT: " + enemyIDsToSpawnQueue.Count);
            //Spawn Enemies
            if (spawnEnemies == true && enemyIDsToSpawnQueue.Count > 0)
            {
                for (int i = 0; i < enemyIDsToSpawnQueue.Count; i++)
                {
                    EnemySpawner.SummonEnemy(enemyIDsToSpawnQueue.Dequeue());

                    spawnedEnemiesCount++;
                    if (spawnedEnemiesCount >= enemyKillCount)
                    {
                        spawnEnemies = false;
                        enemyIDsToSpawnQueue.Clear();
                        CancelInvoke("SpawnTest");
                    }
                }
            }

            //Spawn Towers

            //Move Enemies
            NativeArray<Vector3> nodesToUse = new NativeArray<Vector3>(nodePositions, Allocator.TempJob);
            NativeArray<float> enemySpeeds = new NativeArray<float>(EnemySpawner.enemiesInGame.Count, Allocator.TempJob);
            NativeArray<int> nodeIndices = new NativeArray<int>(EnemySpawner.enemiesInGame.Count, Allocator.TempJob);
            TransformAccessArray enemyAccess = new TransformAccessArray(EnemySpawner.enemiesInGameTransform.ToArray(), 2);

            for (int i = 0; i < EnemySpawner.enemiesInGame.Count; i++)
            {
                enemySpeeds[i] = EnemySpawner.enemiesInGame[i].speed;
                nodeIndices[i] = EnemySpawner.enemiesInGame[i].nodeIndex;
            }

            MoveEnemiesJob moveJob = new MoveEnemiesJob
            {
                nodePositions = nodesToUse,
                enemySpeed = enemySpeeds,
                nodeIndex = nodeIndices,
                deltaTime = Time.deltaTime //can't access Time.deltaTime while multithreading, so grab it here!
            };

            JobHandle moveJobHandle = moveJob.Schedule(enemyAccess);
            moveJobHandle.Complete();

            for (int i = 0; i < EnemySpawner.enemiesInGame.Count; i++)
            {
                EnemySpawner.enemiesInGame[i].nodeIndex = nodeIndices[i];

                if (EnemySpawner.enemiesInGame[i].nodeIndex == nodePositions.Length)
                {
                    EnqueueEnemyToRemove(EnemySpawner.enemiesInGame[i]);
                }
            }

            enemySpeeds.Dispose();
            nodeIndices.Dispose();
            enemyAccess.Dispose();
            nodesToUse.Dispose();

            //Tick Towers
            foreach(TowerBehaviour tower in towersInGame)
            {
                /*if (tower.GetType().IsSubclassOf(typeof(FlamethrowerTower)))
                {
                    FlamethrowerTower flameTower = (FlamethrowerTower)tower;
                    flameTower.target2 = TowerTargeting.GetTarget(flameTower, TowerTargeting.TargetType.Last);
                }*/
                tower.target = TowerTargeting.GetTarget(tower, TowerTargeting.TargetType.First);
                tower.Tick();
            }

            //Apply Effects
            if (effectsQueue.Count > 0)
            {
                for (int i = 0; i < effectsQueue.Count; i++)
                {
                    AppliedEffect currentDamage = effectsQueue.Dequeue();

                    //if the enemy doesn't have the effect, then it is added to the enemy; if not, reset its expireTime
                    Effect existingEffectOnEnemy = currentDamage.enemyToAffect.activeEffects.Find(x => x.effectName == currentDamage.effectToApply.effectName);
                    if (existingEffectOnEnemy == null)
                    {
                        currentDamage.enemyToAffect.activeEffects.Add(currentDamage.effectToApply);
                    }
                    else
                    {
                        existingEffectOnEnemy.expireTime = currentDamage.effectToApply.expireTime;
                    }
                }
            }

            //Tick Enemies
            foreach (Enemy currentEnemy in EnemySpawner.enemiesInGame)
            {
                currentEnemy.Tick();
            }

            //Damage Enemies
            if (damageData.Count > 0)
            {
                for (int i = 0; i < damageData.Count; i++)
                {
                    EnemyDamage currentDamage = damageData.Dequeue();

                    if (currentDamage.target.Health > 0)
                    {
                        currentDamage.target.Health -= currentDamage.totalDamage / currentDamage.resistance;

                        //currently, we only add money upon an enemy's death
                        //uncomment this to add more money based on damage dealt
                        //playerStats.AddMoney((int)currentDamage.totalDamage); 

                        if (currentDamage.target.Health <= 0 && !enemiesToRemoveQueue.Contains(currentDamage.target))
                        {
                            EnqueueEnemyToRemove(currentDamage.target);
                        }
                    }
                }
            }

            //Remove Enemies
            if (enemiesToRemoveQueue.Count > 0)
            {
                Debug.Log("REMOVE QUEUE COUNT: " + enemiesToRemoveQueue.Count + " @ " + Time.time);
                for (int i = 0; i < enemiesToRemoveQueue.Count; i++)
                {
                    //remove this line for a damage-focused economy system
                    playerStats.AddMoney(enemiesToRemoveQueue.Peek().reward);
                    EnemySpawner.RemoveEnemy(enemiesToRemoveQueue.Dequeue());

                    UpdateEnemyCount();
                    if (currEnemyKillCount >= enemyKillCount)
                    {
                        enemiesToRemoveQueue.Clear();
                        damageData.Clear();
                        UpdateWaveCount();
                    }
                }
            }

            //Remove Towers

            yield return null;
        }
    }

    public static void EnqueueEnemyIDToSummon(int id)
    {
        enemyIDsToSpawnQueue.Enqueue(id);
    }

    public static void EnqueueDamageData(EnemyDamage newEnemyDamage)
    {
        damageData.Enqueue(newEnemyDamage);
    }

    public static void EnqueueEffectToApply(AppliedEffect appliedEffect)
    {
        effectsQueue.Enqueue(appliedEffect);
    }

    public static void EnqueueEnemyToRemove(Enemy enemyToRemove)
    {
        enemiesToRemoveQueue.Enqueue(enemyToRemove);
    }
}

public struct EnemyDamage
{
    public Enemy target;
    public float totalDamage;
    public float resistance;

    public EnemyDamage(Enemy newTarget, float newDamage, float newRes)
    {
        target = newTarget;
        totalDamage = newDamage;
        resistance = newRes;
    }
}

[System.Serializable]
public class Effect
{
    public string effectName;

    public float damage;
    public float damageRate;
    public float damageDelay;

    public float expireTime;

    public Effect(string newEffectName, float newDamage, float newDamageRate, float newExpireTime)
    {
        effectName = newEffectName;
        damage = newDamage;
        damageRate = newDamageRate;
        damageDelay = 1f / newDamageRate;
        expireTime = newExpireTime;
    }
}

public struct AppliedEffect
{
    public Enemy enemyToAffect;
    public Effect effectToApply;

    public AppliedEffect(Enemy newEnemy, Effect newEffect)
    {
        enemyToAffect = newEnemy;
        effectToApply = newEffect;
    }
}

public struct MoveEnemiesJob : IJobParallelForTransform
{
    [NativeDisableParallelForRestriction] public NativeArray<Vector3> nodePositions;
    [NativeDisableParallelForRestriction] public NativeArray<float> enemySpeed;
    [NativeDisableParallelForRestriction] public NativeArray<int> nodeIndex;

    public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        if (nodeIndex[index] < nodePositions.Length)
        {
            Vector3 positionToMoveTo = nodePositions[nodeIndex[index]];
            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, enemySpeed[index] * deltaTime);

            if (transform.position == positionToMoveTo)
            {
                nodeIndex[index]++;
            }
        }
    }
}
