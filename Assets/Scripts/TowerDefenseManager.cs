using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;

public enum Phase{ None, Build, Defend, Repair }

public class TowerDefenseManager : MonoBehaviour
{
    public static List<TowerBehaviour> towersInGame;
    private static Queue<TowerBehaviour> towersToRemoveQueue;
    public static Vector3[] nodePositions;
    public static float[] nodeDistances;
    public static int waveCount = 1;
    [SerializeField] public static bool isGameOver = false;

    private static Queue<int> enemyIDsToSpawnQueue;
    private static Queue<Enemy> enemiesToRemoveQueue;
    private static Queue<EnemyDamage> damageData;
    private static Queue<AppliedEffect> effectsQueue;

    private PlayerStats playerStats;

    public Transform nodeParent;
    public bool continueLoop = true;
    public bool spawnEnemies = true;

    [SerializeField] GameObject colliderObject;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] int enemyRemovedCount;
    [SerializeField] int currEnemyKillCount;
    int spawnedEnemiesCount = 0;

    [SerializeField] Phase currPhase;

    // Start is called before the first frame update
    void Start()
    {
        towersInGame = new List<TowerBehaviour>();
        towersToRemoveQueue = new Queue<TowerBehaviour>();
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

        ResetGameStatistics();
        UpdateWaveCount(1);

        StartCoroutine(GameplayLoop());

        //InvokeRepeating("SpawnTest", 0f, 1f);
        //InvokeRepeating("RemoveTest", 0f, 2f);
    }

    void ResetGameStatistics()
    {
        gameOverScreen.SetActive(false);
        isGameOver = false;
        waveCount = 1;
        continueLoop = true;
        towersInGame.Clear();
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
        else
            waveCount = newWaveNum;

        enemyRemovedCount = (waveCount * 5);
        currEnemyKillCount = 0;
        spawnedEnemiesCount = 0;

        playerStats.DisplayWaveCount(waveCount);
        playerStats.DisplayEnemyCount(enemyRemovedCount);

        spawnEnemies = true;
        //Debug.Log("Fake Round Setup: WaveCount: " + waveCount + " // EnemyRemovedCount: " + enemyRemovedCount + 
        //    "// SpawnedEnemiesCount: " + spawnedEnemiesCount + //"// EnemiesInGameCount: " + enemyRemovedCount + 
        //    " // SpawnEnemies: " + spawnEnemies);
        InvokeRepeating("SpawnTest", 0f, 1f);
    }

    public void UpdateEnemyCount()
    {
        currEnemyKillCount++;
        playerStats.DisplayEnemyCount(enemyRemovedCount - currEnemyKillCount);
    }

    IEnumerator GameplayLoop()
    {
        while(continueLoop == true)
        {
            if (isGameOver)
            {
                StartCoroutine(GameOverSequence());
                continueLoop = false;
                break;
            }
            //Debug.Log("QUEUE COUNT: " + enemyIDsToSpawnQueue.Count);
            //Spawn Enemies
            //Debug.Log("Spawn Checking -- SpawnEnemies: " + spawnEnemies + " // EnemyIDsCount: " + enemyIDsToSpawnQueue.Count);
            if (spawnEnemies == true && enemyIDsToSpawnQueue.Count > 0)
            {
                for (int i = 0; i < enemyIDsToSpawnQueue.Count; i++)
                {
                    Debug.Log("Spawning " + enemyIDsToSpawnQueue.Peek());
                    EnemySpawner.SummonEnemy(enemyIDsToSpawnQueue.Dequeue());

                    spawnedEnemiesCount++;
                    if (spawnedEnemiesCount >= enemyRemovedCount)
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
                        currentDamage.target.Health -= currentDamage.totalDamage * currentDamage.resistance;

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
                    if (currEnemyKillCount >= enemyRemovedCount)
                    {
                        enemiesToRemoveQueue.Clear();
                        damageData.Clear();
                        UpdateWaveCount();
                    }
                }
            }

            //Remove Towers
            if (towersToRemoveQueue.Count > 0)
            {
                Debug.Log("REMOVE QUEUE COUNT: " + towersToRemoveQueue.Count + " @ " + Time.time);
                for (int i = 0; i < towersToRemoveQueue.Count; i++)
                {
                    RemoveTower(towersToRemoveQueue.Dequeue());
                }
            }

            yield return null;
        }
    }
    IEnumerator GameOverSequence()
    {
        gameOverScreen.SetActive(true);
        yield return new WaitForSeconds(4f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuVR", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public static void BeginGameOverSequence()
    {
        isGameOver = true;
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

    public static void EnqueueTowerToRemove(TowerBehaviour towerToRemove)
    {
        if (!towersToRemoveQueue.Contains(towerToRemove))
            towersToRemoveQueue.Enqueue(towerToRemove);
    }

    void RemoveTower(TowerBehaviour towerToRemove)
    {
        towersInGame.Remove(towerToRemove);

        Destroy(towerToRemove.gameObject);
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

public enum EffectType { Damage, Slow, Shock}

[System.Serializable]
public class Effect
{
    public string effectName;

    public EffectType effectType;

    public float damage;
    public float damageRate;
    public float damageDelay;
    public ElementType element;

    public float slowAmount;

    public float stopIntervalTime;
    public float resumeIntervalTime;
    public float stopExpireTime;

    public float origSpeed;
    public float expireTime;

    //Damage Over Time Effect
    public Effect(string newEffectName, float newDamage, float newDamageRate, ElementType newElement, float newExpireTime)
    {
        effectName = newEffectName;
        damage = newDamage;
        damageRate = newDamageRate;
        element = newElement;
        damageDelay = 1f / newDamageRate;
        expireTime = newExpireTime;

        effectType = EffectType.Damage;
    }

    //Slow Effect
    public Effect(string newEffectName, float newSlowAmount, float newOrigSpeed, float newExpireTime)
    {
        effectName = newEffectName;
        slowAmount = newSlowAmount;
        origSpeed = newOrigSpeed;
        expireTime = newExpireTime;

        effectType = EffectType.Slow;
    }

    //Shock Effect
    public Effect(string newEffectName, float newStopIntervalTime, float newResumeIntervalTime, float newOrigSpeed, float newExpireTime)
    {
        effectName = newEffectName;
        stopIntervalTime = newStopIntervalTime;
        resumeIntervalTime = newResumeIntervalTime;
        origSpeed = newOrigSpeed;
        expireTime = newExpireTime;
        stopExpireTime = stopIntervalTime;

        effectType = EffectType.Shock;
    }


    public EffectType GetEffectType()
    {
        return effectType;
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

                Vector3 positionToRotateTowards = nodePositions[nodeIndex[index]];
                Vector3 newDir = transform.position - positionToRotateTowards;
                transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
            }
        }
    }
}
