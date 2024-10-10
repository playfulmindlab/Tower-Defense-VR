using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using UnityEngine.Jobs;
using System;

public enum Phase{ None = 0, Build = 1, Defend_ChooseJump = 2, Defend = 3, Repair = 4, Intermission = 5, Pause = 100 }

public class TowerDefenseManager : MonoBehaviour
{
    public static TowerDefenseManager instance;
    public static List<TowerBehaviour> towersInGame;
    private static Queue<TowerBehaviour> towersToRemoveQueue;
    public static Vector3[] nodePositions = null;
    public static float[] nodeDistances = null;
    public static int waveCount = 1;
    int levelCount = 1;
    [SerializeField] public static bool isGameOver = false;

    private static Queue<int> enemyIDsToSpawnQueue;
    private static Queue<Enemy> enemiesToRemoveQueue;
    private static Queue<EnemyDamage> damageData;
    private static Queue<AppliedEffect> effectsQueue;
    private static Queue<AppliedTowerEffect> towerEffectsQueue;

    public Transform nodeParent;
    public TargetType currTargetType = TargetType.First;
    [SerializeField] PathsAndNodesPair[] pathAndNodesPairings;
    [SerializeField] Node[] currNodePath;
    public static Vector3[] nodePositions2 = null;
    int wavesTilEndMap = 5;

    [SerializeField] GameObject colliderObject;
    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject victoryScreen;
    [SerializeField] TMPro.TextMeshProUGUI phaseText;
 
    static Phase currPhase;
    public static Phase CurrPhase { get { return currPhase; } set { } }

    SpawnablesEnabler spawnablesEnabler;

    PlayerStats playerStats;
    int enemyRemovedCount;
    int currEnemyKillCount;
    int currEnemiesRemovedCount;
    int spawnedEnemiesCount = 0;

    [SerializeField] float enemySpawnTimes = 1.5f;
    [SerializeField] float intermissionTime = 2f;
    [SerializeField] bool continueLoop = true;
    [SerializeField] bool spawnEnemies = true;

    bool gamePaused = false;
    public bool IsGamePaused { get { return gamePaused; } set { } }
    Phase prePausePhase = Phase.None;

    [SerializeField] bool fastToWave = true;
    [SerializeField] int setWave = 1;

    [SerializeField] PhaseControlUI phaseUIController;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else { Debug.Log("Spare TowerDefenseManager Found"); Destroy(this); }

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
        towerEffectsQueue = new Queue<AppliedTowerEffect>();
        playerStats = FindObjectOfType<PlayerStats>();
        spawnablesEnabler = SpawnablesEnabler.instance;

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

        //waveMult = wavesTilLevelWin;
        spawnEnemies = false;

        UpdateNewNodePath(0);
        //UpdateNewNodePath(1);
        //UpdateNewNodePath(2);

        nodePositions2 = NodePathPositions();

        ResetGameStatistics();
        EnemySpawner.Init();

        wavesTilEndMap = EnemySpawner.totalWaves;

        if (fastToWave == true)
        {
            waveCount = setWave;
            levelCount = setWave / 5;
        }

        StartCoroutine(GameplayLoop());
    }

    //Note: Update here is ONLY meant to be used for keycode entry for testing. All other functions
    //will update in the GameplayLoop() IEnumerator
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ChangePhase(Phase.Build);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            ChangePhase(Phase.Defend);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

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
                    SpawnablesEnabler.instance.DisableAllTowers();
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
        currEnemyKillCount = 0;
        continueLoop = true;
        spawnablesEnabler.WaveUpdate(waveCount);

        towersInGame.Clear();
        enemyIDsToSpawnQueue.Clear();
        towersToRemoveQueue.Clear();
        damageData.Clear();
        effectsQueue.Clear();
        towerEffectsQueue.Clear();
    }

    void SpawnEnemies() 
    {
        //int newID = EnemySpawner.GetValidIDToSpawn();
        int newID = EnemySpawner.GetNextIDToSpawn();
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
        //phaseUIController
        //currPhase = (Phase)phaseInt;
        ChangePhase((Phase)phaseInt);
    }

    void UpdateNewNodePath(int newPath)
    {
        Debug.Log("NEW PATH: " + newPath);
        //enable path, make it visible - the path is technically aesthetic
        pathAndNodesPairings[newPath].path.SetActive(true);
        pathAndNodesPairings[newPath].minimapPath.SetActive(true);
        foreach (Transform childPath in pathAndNodesPairings[newPath].path.transform)
        {
            //set children's layer from "LevelUpPath" to "Path"
            childPath.gameObject.layer = 11;
        }
        foreach (Transform childMiniPath in pathAndNodesPairings[newPath].minimapPath.transform)
        {
            //set children's layer from "LevelUpPath" to "Path"
            childMiniPath.gameObject.layer = 11;
        }

        //Safety Check - if "newPath" is 0, this makes sure the game doesn't break
        if (newPath > 0)
        {
            pathAndNodesPairings[newPath - 1].nodeHolder.SetActive(false);
        }
        pathAndNodesPairings[newPath].nodeHolder.SetActive(true);
        currNodePath = pathAndNodesPairings[newPath].Nodes;

        if (currNodePath == null || currNodePath.Length <= 0)
        {
            Debug.LogError("Error getting the nodes for the new path!");
            return;
        }

        //safety check to make sure that the first node on the node array IS the starting node
        if (!currNodePath[0].isStart)
        {
            for(int n = 0; n < currNodePath.Length; n++)
            {
                if (currNodePath[n].isStart)
                {
                    currNodePath[n] = currNodePath[0];
                    break;
                }
            }

            currNodePath[0] = pathAndNodesPairings[newPath].FirstNode;
        }
    }

    void GetEnemysNewPath(Enemy newEnemy)
    {
        List<Node> newNodePath = new List<Node>();
        List<int> newIndexPath = new List<int>();

        newNodePath.Add(currNodePath[0]);
        newIndexPath.Add(0);

        while (newNodePath[newNodePath.Count - 1].CanGetNextNode())
        {
            newNodePath.Add(newNodePath[newNodePath.Count - 1].GetNextNode(newNodePath));
            newIndexPath.Add(System.Array.IndexOf(currNodePath, newNodePath[newNodePath.Count - 1]));
        }

        Debug.Log(newEnemy.name);
        Debug.Log(newNodePath);
        newEnemy.currNodePath = newNodePath.ToArray();
        newEnemy.currNodeIndices = newIndexPath.ToArray();

        //-----------------------

        /*
        string outputPath = newEnemy.gameObject.name + "'s New Path: ";
        for (int i = 0; i < newEnemy.currNodePath.Length; i++)
        {
            outputPath += newEnemy.currNodePath[i].gameObject.name + " > ";
        }
        Debug.Log(outputPath);

        outputPath = newEnemy.gameObject.name + "'s New Int Path: ";
        for (int i = 0; i < newEnemy.currNodeIndices.Length; i++)
        {
            outputPath += newEnemy.currNodeIndices[i] + " > ";
        }
        Debug.Log(outputPath);
        */
    }

    void UpdateWaveCount(int newWaveNum = -1)
    {
        if (gamePaused != false)
        {
            return;
        }

        char nextStep = EnemySpawner.afterWaveStatus[waveCount - 1];

        if (newWaveNum < 0)
            waveCount++;
        else
            waveCount = newWaveNum;

        //tower unlock system
        spawnablesEnabler.WaveUpdate(waveCount);

        UpdateGameManagerStats();

        if (newWaveNum < 0)
        {
            //char nextStep = EnemySpawner.afterWaveStatus[waveCount - 1];
            Debug.Log("NEXT STEP: " + nextStep.ToString());
            switch (nextStep)
            {
                case 'B': //Break: intermission
                    DataEvent newEvent = new DataEvent("Intermission", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
                    EventManager.instance.RecordNewEvent(newEvent);

                    ChangePhase(Phase.Build);
                    phaseUIController.ToggleDefendButton(true);
                    return;
                //break;

                case 'L': // Level-up
                    DataEvent newEvent2 = new DataEvent("Level Clear", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
                    EventManager.instance.RecordNewEvent(newEvent2);

                    UpdateLevelCount();
                    phaseUIController.ToggleDefendButton(true);
                    return;
                //break;

                default: //all other cases: advance to next wave uninterrupted
                    //MiniMapTowerPlacement
                    break;
            }
        }

        Debug.Log("WENT THRU ANYWAYS");

        if (waveCount <= EnemySpawner.numEnemiesInWaves.Length)
        {
            StartCoroutine(WaitTime());
        }
    }

    IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(intermissionTime);

        enemyRemovedCount = EnemySpawner.numEnemiesInWaves[waveCount - 1];// (waveCount * numEnemiesPerWave);
        currEnemiesRemovedCount = 0;
        spawnedEnemiesCount = 0;

        playerStats.DisplayWaveCount(waveCount);
        playerStats.DisplayEnemyCount(enemyRemovedCount);

        spawnEnemies = true;

        AudioManager.instance.PlaySFXArray("NewWaveSound", new Vector3(20, 10, 0));
        InvokeRepeating("SpawnEnemies", 0f, enemySpawnTimes);

        DataEvent newEvent = new DataEvent("Wave Start", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
        EventManager.instance.RecordNewEvent(newEvent);
    }

    void UpdateLevelCount()
    {
        if (gamePaused != false)
        {
            return;
        }

        if (levelCount >= wavesTilEndMap)
        {
            StartCoroutine(LevelVictorySequence());
            continueLoop = false;
            return;
        }

        levelCount++;
        //wavesTilLevelWin += waveMult;

        UpdateGameManagerStats();
        UpdateNewNodePath(levelCount - 1);

        nodePositions2 = NodePathPositions();

        ChangePhase(Phase.Build);
    }

    void UpdateEnemyCount()
    {
        currEnemiesRemovedCount++;
        playerStats.DisplayEnemyCount(enemyRemovedCount - currEnemiesRemovedCount);
    }

    Vector3[] NodePathPositions()
    {
        List<Vector3> newNodePositionList = new List<Vector3>();

        for (int n = 0; n < currNodePath.Length; n++)
        {
            newNodePositionList.Add(currNodePath[n].gameObject.transform.position);
        }

        return newNodePositionList.ToArray();
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

                    Enemy newEnemy = EnemySpawner.SummonEnemy(enemyIDsToSpawnQueue.Dequeue());
                    //newEnemy.gameObject.name += spawnedEnemiesCount;
                    GetEnemysNewPath(newEnemy);
                    //EnemySpawner.SummonRandomEnemy();

                    spawnedEnemiesCount++;
                    //Debug.Log("KEEP SPAWNING ENEMIES??? " + spawnedEnemiesCount + " / " + EnemySpawner.numEnemiesInWaves[waveCount - 1]);
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
                NativeArray<Vector3> nodesToUse = new NativeArray<Vector3>(nodePositions2, Allocator.TempJob);
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

                    Debug.Log("INDEX COMP: " + EnemySpawner.enemiesInGame[i].nodeIndex + " vs " + nodePositions2.Length);
                    /*if (EnemySpawner.enemiesInGame[i].nodeIndex >= nodePositions2.Length)
                    {
                        GameManager.instance.LogNewEvent("Enemy Finished", EnemySpawner.enemiesInGame[i].gameObject, 
                            transform.position, GameControlManager.instance.IsJumped, EnemySpawner.enemiesInGame[i].Health);
                        EnqueueEnemyToRemove(EnemySpawner.enemiesInGame[i]);
                    }*/
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
                    if (currTargetType != TargetType.None)
                        tower.target = TowerTargeting.GetTarget(tower, currTargetType);
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

            //Apply Tower Effects
            if (!gamePaused)
            {
                if (towerEffectsQueue.Count > 0)
                {
                    for (int i = 0; i < towerEffectsQueue.Count; i++)
                    {
                        AppliedTowerEffect currentEffect = towerEffectsQueue.Dequeue();

                        //if the enemy doesn't have the effect, then it is added to the enemy; if not, reset its expireTime
                        Effect existingEffectOnTower = currentEffect.towerToAffect.activeEffects.Find(x => x.effectName == currentEffect.effectToApply.effectName);
                        if (existingEffectOnTower == null)
                        {
                            currentEffect.towerToAffect.activeEffects.Add(currentEffect.effectToApply);
                        }
                        else
                        {
                            existingEffectOnTower.expireTime = currentEffect.effectToApply.expireTime;
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

                        DamagePopupGenerator.instance.SpawnNewPopup(currentDamage.target, currentDamage.totalDamage);

                        //currently, we only add money upon an enemy's death
                        //uncomment this to add more money based on damage dealt
                        //playerStats.AddMoney((int)currentDamage.totalDamage); 

                        if (currentDamage.target.Health <= 0 && !enemiesToRemoveQueue.Contains(currentDamage.target))
                        {
                            currEnemyKillCount++;
                            playerStats.UpdateKillCountText(currEnemyKillCount);
                            GameManager.instance.LogNewEvent("Enemy Death", currentDamage.target.gameObject, currentDamage.target.gameObject.transform.position, GameControlManager.instance.IsJumped);
                            EnqueueEnemyToRemove(currentDamage.target);
                        }
                    }
                }
            }

            //Remove Enemies
            if (enemiesToRemoveQueue.Count > 0)
            {
                //Debug.Log("REMOVE QUEUE COUNT: " + enemiesToRemoveQueue.Count + " @ " + Time.time);
                for (int i = 0; i < enemiesToRemoveQueue.Count; i++)
                {
                    AudioManager.instance.PlaySFXArray("EnemyDie", enemiesToRemoveQueue.Peek().gameObject.transform.position);
                    //remove this line for a damage-focused economy system
                    playerStats.AddMoney(enemiesToRemoveQueue.Peek().reward);
                    EnemySpawner.RemoveEnemy(enemiesToRemoveQueue.Dequeue());

                    UpdateEnemyCount();
                    Debug.Log("Curr Wave Progress: " + currEnemiesRemovedCount + " out of " + EnemySpawner.numEnemiesInWaves[waveCount - 1]);
                    if (currEnemiesRemovedCount >= EnemySpawner.numEnemiesInWaves[waveCount - 1])
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuXR", UnityEngine.SceneManagement.LoadSceneMode.Single);
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

    public static void EnqueueTowerEffectToApply(AppliedTowerEffect appliedEffect)
    {
        towerEffectsQueue.Enqueue(appliedEffect);
    }

    public static void EnqueueEnemyToRemove(Enemy enemyToRemove)
    {
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

    public void SwapTargetType(int newTargetTypeIndex)
    {
        TargetType newTargetType = (TargetType) newTargetTypeIndex;
        currTargetType = newTargetType;
    }

    void UpdateGameManagerStats()
    {
        GameManager.instance.ChangePathWave(levelCount, waveCount);
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
public class PathsAndNodesPair
{
    public GameObject path;
    public GameObject minimapPath;
    public GameObject nodeHolder;

    public Node FirstNode { get { return nodeHolder.transform.GetChild(0).GetComponent<Node>(); } }
    public Node[] Nodes { get { return nodeHolder.GetComponentsInChildren<Node>(); } }
}

public enum EffectType { Damage, Slow, Shock, Stun }

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
    public Effect(string newEffectName, float newDamage, float newDamageRate, float newStopIntervalTime, float newResumeIntervalTime, float newOrigSpeed, float newExpireTime)
    {
        effectName = newEffectName;
        damage = newDamage;
        damageRate = newDamageRate;
        stopIntervalTime = newStopIntervalTime;
        resumeIntervalTime = newResumeIntervalTime;
        origSpeed = newOrigSpeed;
        expireTime = newExpireTime;
        stopExpireTime = stopIntervalTime;

        element = ElementType.Electric;
        effectType = EffectType.Shock;
    }

    //Stun Effect (towers)
    public Effect(string newEffectName, float newExpireTime)
    {
        effectName = newEffectName;
        expireTime = newExpireTime;

        effectType = EffectType.Stun;
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

public struct AppliedTowerEffect
{
    public TowerBehaviour towerToAffect;
    public Effect effectToApply;

    public AppliedTowerEffect(TowerBehaviour newTower, Effect newEffect, string sfxName = null)
    {
        towerToAffect = newTower;
        effectToApply = newEffect;

        if (sfxName != null)
            AudioManager.instance.PlaySFXArray(sfxName, newTower.transform.position);
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
            string debugString = "";

            foreach (var x in nodeIndex)
            {
                debugString += x.ToString() + " > ";
            }
            
           // Debug.Log
            Vector3 positionToMoveTo = nodePositions[nodeIndex[index]];
            transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, enemySpeed[index] * deltaTime);

            if (transform.position == positionToMoveTo && index < nodeIndex.Length)
            {
                //nodeIndex[index]++;             
                nodeIndex[index] = EnemySpawner.enemiesInGame[index].GetNextIndex();

                Vector3 positionToRotateTowards = nodePositions[nodeIndex[index]];

                if (positionToMoveTo != positionToRotateTowards)
                {
                    Vector3 newDir = transform.position - positionToRotateTowards;
                    transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);
                }
                else
                {
                    EnemySpawner.enemiesInGame[index].ToggleEndOfPath();
                }
            }

        }
    }
}
