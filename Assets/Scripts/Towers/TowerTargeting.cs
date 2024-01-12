using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

public class TowerTargeting
{
    public enum TargetType { First, Last, Closest, Strongest, Weakest }

    public static Enemy GetTarget(TowerBehaviour currentTower, TargetType targetMethod)
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(currentTower.transform.position, currentTower.range, currentTower.enemiesLayer);
        if (enemiesInRange.Length == 0) { return null; }

        NativeArray<EnemyData> enemiesToCalculate = new NativeArray<EnemyData>(enemiesInRange.Length, Allocator.TempJob);
        NativeArray<Vector3> nodePositions = new NativeArray<Vector3>(TowerDefenseManager.nodePositions, Allocator.TempJob);
        NativeArray<float> nodeDistances = new NativeArray<float>(TowerDefenseManager.nodeDistances, Allocator.TempJob);
        NativeArray<int> enemyToIndex = new NativeArray<int>(new int[] { -1 }, Allocator.TempJob);
        int enemyIndexToReturn = -1;

        for (int i = 0; i < enemiesToCalculate.Length; i++)
        {
            Enemy currentEnemy = enemiesInRange[i]./*transform.parent.*/GetComponent<Enemy>();
            int enemyIndexInList = EnemySpawner.enemiesInGame.FindIndex(x => x == currentEnemy);
            enemiesToCalculate[i] = new EnemyData(currentEnemy.transform.position, currentEnemy.nodeIndex, currentEnemy.Health, enemyIndexInList);
        }

        SearchForEnemy enemySearchJob = new SearchForEnemy
        {
            _enemiesToCalculate = enemiesToCalculate,
            _nodePositions = nodePositions,
            _nodeDistances = nodeDistances,
            _enemyToIndex = enemyToIndex,
            towerPosition = currentTower.transform.position,
            //compareValue = Mathf.Infinity,
            targetingType = (int)targetMethod
        };

        switch ((int)targetMethod)
        {
            case 0: // First
                enemySearchJob.compareValue = Mathf.Infinity;
                break;
            case 1: // Last
                enemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
            case 2: // Closest
                goto case 0;
            case 3: // Strongest
                goto case 1;
            case 4: // Weakest
                goto case 0;
        }

        JobHandle dependency = new JobHandle();
        JobHandle searchJobhandle = enemySearchJob.Schedule(enemiesToCalculate.Length, dependency);

        searchJobhandle.Complete();

        if (enemyToIndex[0] != -1) {
            enemyIndexToReturn = enemiesToCalculate[enemyToIndex[0]].enemyIndex;

            enemiesToCalculate.Dispose();
            nodePositions.Dispose();
            nodeDistances.Dispose();
            enemyToIndex.Dispose();

            return EnemySpawner.enemiesInGame[enemyIndexToReturn];
        }

        enemiesToCalculate.Dispose();
        nodePositions.Dispose();
        nodeDistances.Dispose();
        enemyToIndex.Dispose();
        return null;  
    }

    struct EnemyData
    {
        public Vector3 enemyPosition;
        public int nodeIndex;
        public int enemyIndex;
        public float health;

        public EnemyData(Vector3 position, int index, float hp, int eIndex)
        {
            enemyPosition = position;
            nodeIndex = index;
            enemyIndex = eIndex;
            health = hp;
        }
    }

    struct SearchForEnemy : IJobFor
    {
        [ReadOnly] public NativeArray<EnemyData> _enemiesToCalculate;
        [ReadOnly] public NativeArray<Vector3> _nodePositions;
        [ReadOnly] public NativeArray<float> _nodeDistances;
        [NativeDisableParallelForRestriction] public NativeArray<int> _enemyToIndex;

        public Vector3 towerPosition;
        public float compareValue;
        public int targetingType;

        public void Execute(int index)
        {
            float currentEnemyDistanceToEnd = 0;
            float distanceToEnemy = 0;

            switch (targetingType)
            {
                case 0: // First
                    currentEnemyDistanceToEnd = GetDistanceToEnd(_enemiesToCalculate[index]);
                    if (currentEnemyDistanceToEnd < compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = currentEnemyDistanceToEnd;
                    }
                    break;
                case 1: // Last
                    currentEnemyDistanceToEnd = GetDistanceToEnd(_enemiesToCalculate[index]);
                    if (currentEnemyDistanceToEnd > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = currentEnemyDistanceToEnd;
                    }
                    break;
                case 2: // Closest
                    distanceToEnemy = Vector3.Distance(towerPosition, _enemiesToCalculate[index].enemyPosition);
                    if (distanceToEnemy < compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = distanceToEnemy;
                    }
                    break;
                case 3: //Strongest
                    if (_enemiesToCalculate[index].health > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = _enemiesToCalculate[index].health;
                    }
                    break;
                case 4: //Weakest
                    if (_enemiesToCalculate[index].health < compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = _enemiesToCalculate[index].health;
                    }
                    break;
            }
        }

        private float GetDistanceToEnd(EnemyData enemyToEvaluate)
        {
            if (enemyToEvaluate.nodeIndex >= _nodePositions.Length) return 0;

            float finalDistance = Vector3.Distance(enemyToEvaluate.enemyPosition, _nodePositions[enemyToEvaluate.nodeIndex]);

            for (int i = enemyToEvaluate.nodeIndex; i < _nodeDistances.Length; i++)
            {
                finalDistance += _nodeDistances[i];
            }

            return finalDistance;
        }
    }
}
