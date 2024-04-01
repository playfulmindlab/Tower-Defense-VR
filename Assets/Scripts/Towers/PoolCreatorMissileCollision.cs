using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolCreatorMissileCollision : MonoBehaviour
{
    [SerializeField] private MissileDamage baseClass;
    [SerializeField] private ParticleSystem explosionSystem;
    [SerializeField] private ParticleSystem missileSystem;
    [SerializeField] private float explosionRadius;
    private List<ParticleCollisionEvent> missileCollisions;

    public GameObject poolPrefab;

    // Start is called before the first frame update
    void Start()
    {
        missileCollisions = new List<ParticleCollisionEvent>();
        Physics.IgnoreLayerCollision(0, 10);
    }

    private void OnParticleCollision(GameObject other)
    {
        missileSystem.GetCollisionEvents(other, missileCollisions);

        for (int collisionEvent = 0; collisionEvent < missileCollisions.Count; collisionEvent++)
        {
            explosionSystem.transform.position = missileCollisions[collisionEvent].intersection;
            explosionSystem.Play();
            
            Collider[] enemiesInRadius = Physics.OverlapSphere(missileCollisions[collisionEvent].intersection, explosionRadius, 1 << LayerMask.NameToLayer("Enemy"));
            //Debug.Log("Enemies: " + enemiesInRadius.Length + " / On: " + LayerMask.NameToLayer("Enemy"));

            for (int i = 0; i < enemiesInRadius.Length; i++)
            {
                //Debug.Log("Collider: " + enemiesInRadius[i] + " Layer: " + enemiesInRadius[i].gameObject.layer);
                Enemy enemyToDamage = EnemySpawner.enemyTransformPairs[enemiesInRadius[i].transform];
                EnemyDamage damageToApply = new EnemyDamage(enemyToDamage, baseClass.DamageValue, enemyToDamage.GetResistanceModifier(baseClass.GetAttackType));
                TowerDefenseManager.EnqueueDamageData(damageToApply);
            }

            Vector3 spawnPoint = missileCollisions[collisionEvent].intersection + (Vector3.down * missileSystem.main.startSizeYMultiplier / 2f);
            Instantiate(poolPrefab, spawnPoint, Quaternion.identity);
        }
    }
}

