using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using System;

public enum Phase{ None = 0, Build = 1, Defend_ChooseJump = 2, Defend = 3, Repair = 4, Pause = 100 }

public class TowerDefenseManager : MonoBehaviour
{
    public static TowerDefenseManager instance;
    public static List<TowerBehaviour> towersInGame;
    private static Queue<TowerBehaviour> towersToRemoveQueue;
    public static Vector3[] nodePositions = null;
    public Node startingNode;
    //public static Dictionary<int, Node[]> enemyPathing = null;
    public static float[] nodeDistances = null;
    public static int waveCount = 1;
    int levelCount = 1;
    [SerializeField] public static bool isGameOver = false;

    private static Queue<int> enemyIDsToSpawnQueue;
    private static Queue<Enemy> enemiesToRemoveQueue;
    private static Queue<EnemyDamage> damageData;
    private static Queue<AppliedEffect> effectsQueue;

    public Transform nodeParent;
    [SerializeField] System.Tuple<GameObject, GameObject>[] pathNodePairings;
    //[SerializeField] int numEnemiesPerWave = 10;
    public int wavesTilLevelWin = 5;
    public int levelsTilMapWin = 3;
    int waveMult;

    [SerializeField] GameObject colliderObject;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject victoryScreen;
    [SerializeField] TMPro.TextMeshProUGUI phaseText;
 
    static Phase currPhase;
    public static Phase CurrPhase { get { return currPhase; } set { } }

    PlayerStats playerStats;
    int enemyRemovedCount;
    int currEnemyKillCount;
    int spawnedEnemiesCount = 0;
    [SerializeField] bool continueLoop = true;
    [SerializeField] bool spawnEnemies = true;

    bool gamePaused = false;
    public bool IsGamePaused { get { return gamePaused; } set { } }
    Phase prePausePhase = Phase.None;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else { Debug.Log("SpareFound"); Destroy(this); }

        if (EventManager.instance != null)
        {
            DataEvent newEvent = new DataEvent("Map Start", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
            EventManager.instance.RecordNewEvent(newEvent);
        }

        ChangePhase(Phase.Build);
        towersInGame = new List<TowerBehaviour>();
        towersToRemoveQueue = new Queue<TowerBehaviour>();
        enemyIDsToSpawnQueue = new Queue<int>();
        enemiesToRemoveQueue = new Queue<Enemy>();
        damageData = new Queue<EnemyDamage>();
        effectsQueue = new Queue<AppliedEffect>();
        playerStats = FindObjectOfType<PlayerStats>();

        if (nodePositions == null)
        {
            nodePositions = new Vector3[nodeParent.childCount];
            for (int i = 0; i < nodePositions.Length; i++)
            {
                nodePositions[i] = nodeParent.GetChild(i).position;
            }
        }

        if (nodeDistances == null)
        {
            nodeDistances = new float[nodePositions.Length - 1];
            for (int i = 0; i < nodeDistances.Length; i++)
            {
                nodeDistances[i] = Vector3.Distance(nodePositions[i], nodePositions[i + 1]);
            }
        }

        Debug.Log(nodePositions.Length + " // " + nodeDistances.Length);

        waveMult = wavesTilLevelWin;
        spawnEnemies = false;

        ResetGameStatistics();
        EnemySpawner.Init();
        //UpdateWaveCount(1);

        StartCoroutine(GameplayLoop());

        //InvokeRepeating("SpawnTest", 0f, 1f);
        //InvokeRepeating("RemoveTest", 0f, 2f);
    }

    /*
    public Node[] CreateNewEnemyPath()
    {
        List<Node> newPathList = new List<Node>();

        newPathList.Add(startingNode);

        while (true)
        {
            Node currNode = newPathList[newPathList.Count - 1];
            Node newNode = currNode.GetNextNode(newPathList);
            newPathList.Add(newNode);

            if (newNode.isEnd)
                break;
        }

        return newPathList.ToArray();
    }
    */

    public void ChangePhase(Phase newPhase)
    {
        if (currPhase == Phase.Pause && prePausePhase != newPhase)
        {
            return;
        }

        switch (newPhase)
        {
            case Phase.Build:
                spawnEnemies = false;
                phaseText.text = "Build";
                break;

            case Phase.Defend_ChooseJump:
                spawnEnemies = true;
                phaseText.text = "Defend (Choose Jump Target)";
                break;

            case Phase.Defend:
                spawnEnemies = true;
                phaseText.text = "Defend";
                if (currPhase != Phase.Pause)
                {
                    DataEvent newEvent = new DataEvent("Level Start", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
                    EventManager.instance.RecordNewEvent(newEvent);

                    UpdateWaveCount(waveCount);
                }
                break;

            case Phase.Repair:
                spawnEnemies = false;
                phaseText.text = "Repair";
                break;

            case Phase.Pause:
                spawnEnemies = false;
                prePausePhase = currPhase;
                phaseText.text = "PAUSED";
                break;

            default:
                break;
        }

        currPhase = newPhase;
    }

    void ResetGameStatistics()
    {
        gameOverScreen.SetActive(false);
        victoryScreen.SetActive(false);
        isGameOver = false;
        waveCount = 1;
        continueLoop = true;

        towersInGame.Clear();
        enemyIDsToSpawnQueue.Clear();
        towersToRemoveQueue.Clear();
        damageData.Clear();
        effectsQueue.Clear();
    }

    void SpawnEnemies() 
    {
        int newID = EnemySpawner.GetValidIDToSpawn();
        EnqueueEnemyIDToSummon(newID); 
    }

    void RemoveTest()
    {
        if (EnemySpawner.enemiesInGame.Count > 0)
        {
            EnemySpawner.RemoveEnemy(EnemySpawner.enemiesInGame[UnityEngine.Random.Range(0, EnemySpawner.enemiesInGame.Count)]);
        }
    }

    public void ChangeCurrentPhaseButton(int phaseInt)
    {
        //currPhase = (Phase)phaseInt;
        ChangePhase((Phase)phaseInt);
    }

    void UpdateNewNodePath()
    {
        //pathCount++;
        startingNode = pathNodePairings[0].Item1.GetComponent<Node>();

    }

    void UpdateWaveCount(int newWaveNum = -1)
    {
        if (gamePaused != false)
        {
            return;
        }

        if (newWaveNum < 0)
            waveCount++;
        else
            waveCount = newWaveNum;

        if (waveCount > wavesTilLevelWin)
        {
            UpdateLevelCount();
            return;
        }

        if (waveCount <= EnemySpawner.numEnemiesInWaves.Length)
        {
            enemyRemovedCount = EnemySpawner.numEnemiesInWaves[waveCount - 1];// (waveCount * numEnemiesPerWave);
            currEnemyKillCount = 0;
            spawnedEnemiesCount = 0;

            playerStats.DisplayWaveCount(waveCount);
            playerStats.DisplayEnemyCount(enemyRemovedCount);

            spawnEnemies = true;

            AudioManager.instance.PlaySFXArray("NewWaveSound", new Vector3(20, 10, 0));
            InvokeRepeating("SpawnEnemies", 0f, 1f);

            //GameManager.instance.LogNewMainMenuEvent("Wave Start");
            DataEvent newEvent = new DataEvent("Wave Start", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
            EventManager.instance.RecordNewEvent(newEvent);
        }
    }

    void UpdateLevelCount()
    {
        if (gamePaused != false)
        {
            return;
        }

        DataEvent newEvent = new DataEvent("Level Clear", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent);

        if (levelCount >= levelsTilMapWin)
        {
            StartCoroutine(LevelVictorySequence());
            continueLoop = false;
            return;
        }

        levelCount++;
        wavesTilLevelWin += waveMult;

        ChangePhase(Phase.Build);
    }

    public void UpdateEnemyCount()
    {
        currEnemyKillCount++;
        playerStats.DisplayEnemyCount(enemyRemovedCount - currEnemyKillCount);
    }

    IEnumerator GameplayLoop()
    {
        while (continueLoop == true)
        {
            //if base health = 0, start Game Over sequence
            if (isGameOver)
            {
                StartCoroutine(GameOverSequence());
                continueLoop = false;
                break;
            }

            //Spawn Enemies
            //Debug.Log("Spawn Checking -- SpawnEnemies: " + spawnEnemies + " // EnemyIDsCount: " + enemyIDsToSpawnQueue.Count);
            if (spawnEnemies == true && enemyIDsToSpawnQueue.Count > 0)
            {
                for (int i = 0; i < enemyIDsToSpawnQueue.Count; i++)
                {
                    Debug.Log("Spawning " + enemyIDsToSpawnQueue.Peek() + " out of " + enemyIDsToSpawnQueue.Count);

                    EnemySpawner.SummonEnemy(enemyIDsToSpawnQueue.Dequeue());
                    //EnemySpawner.SummonRandomEnemy();

                    spawnedEnemiesCount++;
                    Debug.Log("KEEP SPAWNING ENEMIES??? " + spawnedEnemiesCount + " / " + EnemySpawner.numEnemiesInWaves[waveCount - 1]);
                    if (spawnedEnemiesCount >= EnemySpawner.numEnemiesInWaves[waveCount - 1])
                    {
                        spawnEnemies = false;
                        enemyIDsToSpawnQueue.Clear();
                        CancelInvoke("SpawnEnemies");
                    }
                }
            }

            //Spawn Towers

            //Move Enemies
            if (!gamePaused)
            {
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
            }

            //Tick Towers
            if (!gamePaused)
            {
                foreach (TowerBehaviour tower in towersInGame)
                {
                    /*if (tower.GetType().IsSubclassOf(typeof(FlamethrowerTower)))
                    {
                        FlamethrowerTower flameTower = (FlamethrowerTower)tower;
                        flameTower.target2 = TowerTargeting.GetTarget(flameTower, TowerTargeting.TargetType.Last);
                    }*/
                    tower.target = TowerTargeting.GetTarget(tower, TowerTargeting.TargetType.First);
                    tower.Tick();
                }
            }

            //Apply Effects
            if (!gamePaused)
            {
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
            }

            //Tick Enemies
            if (!gamePaused)
            {
                foreach (Enemy currentEnemy in EnemySpawner.enemiesInGame)
                {
                    currentEnemy.Tick();
                }
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
                    AudioManager.instance.PlaySFXArray("EnemyDie", enemiesToRemoveQueue.Peek().gameObject.transform.position);
                    //remove this line for a damage-focused economy system
                    playerStats.AddMoney(enemiesToRemoveQueue.Peek().reward);
                    EnemySpawner.RemoveEnemy(enemiesToRemoveQueue.Dequeue());

                    UpdateEnemyCount();
                    Debug.Log("Curr Wave Progress: " + currEnemyKillCount + " out of " + EnemySpawner.numEnemiesInWaves[waveCount - 1]);
                    if (currEnemyKillCount >= EnemySpawner.numEnemiesInWaves[waveCount - 1])
                    {
                        DataEvent newEvent = new DataEvent("Wave End", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
                        EventManager.instance.RecordNewEvent(newEvent);

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

    IEnumerator LevelVictorySequence()
    {
        victoryScreen.SetActive(true);
        DataEvent newEvent = new DataEvent("Map Clear", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent);

        TogglePause();
        yield return new WaitForSeconds(4f);
        TogglePause();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuXR", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    IEnumerator GameOverSequence()
    {
        DataEvent newEvent = new DataEvent("Player Death", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent);

        gameOverScreen.SetActive(true);
        AudioManager.instance.PlaySFXArray("GameOver", Camera.main.transform.position);
        yield return new WaitForSeconds(4f);

        DataEvent newEvent2 = new DataEvent("Game Quit", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent2);
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
        GameManager.instance.LogNewEvent("Enemy Death", enemyToRemove.gameObject, enemyToRemove.gameObject.transform.position, GameControlManager.instance.IsJumped);
        //DataEvent newEvent = new DataEvent("Enemy Death", enemyToRemove.gameObject, enemyToRemove.gameObject.transform.position, GameControlManager.instance.IsJumped);
        //EventManager.instance.RecordNewEvent(newEvent);
        enemiesToRemoveQueue.Enqueue(enemyToRemove);
    }

    public static void EnqueueTowerToRemove(TowerBehaviour towerToRemove)
    {
        if (!towersToRemoveQueue.Contains(towerToRemove))
            towersToRemoveQueue.Enqueue(towerToRemove);
    }

    void RemoveTower(TowerBehaviour towerToRemove)
    {
        GameManager.instance.LogNewEvent("PPO Destroyed", towerToRemove.gameObject, towerToRemove.gameObject.transform.position, GameControlManager.instance.IsJumped);
        //DataEvent newEvent = new DataEvent("PPO Destroyed", towerToRemove.gameObject, towerToRemove.gameObject.transform.position, GameControlManager.instance.IsJumped);
        //EventManager.instance.RecordNewEvent(newEvent);
        towersInGame.Remove(towerToRemove);

        Destroy(towerToRemove.gameObject);
    }

    public void TogglePause()
    {
        gamePaused = !gamePaused;

        if (gamePaused == true)
        {
            ChangePhase(Phase.Pause);
        }
        else
        {
            ChangePhase(prePausePhase);
        }
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

    public AppliedEffect(Enemy newEnemy, Effect newEffect, string sfxName = null)
    {
        enemyToAffect = newEnemy;
        effectToApply = newEffect;

        if (sfxName != null)
            AudioManager.instance.PlaySFXArray(sfxName, newEnemy.transform.position);
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
