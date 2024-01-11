using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileCollision : MonoBehaviour
{
    [SerializeField] private MissileDamage baseClass;
    [SerializeField] private ParticleSystem explosionSystem;
    [SerializeField] private ParticleSystem missileSystem;
    [SerializeField] private float explositonRadius;
    private List<ParticleCollisionEvent> missileCollisions;

    public float damageValue = 0;
    [SerializeField] bool splashDamage = false;

    // Start is called before the first frame update
    void Start()
    {
        missileCollisions = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        missileSystem.GetCollisionEvents(other, missileCollisions);

        for (int collisionEvent = 0; collisionEvent < missileCollisions.Count; collisionEvent++)
        {
            explosionSystem.transform.position = missileCollisions[collisionEvent].intersection;
            explosionSystem.Play();

            if (splashDamage == true)
            {
                Collider[] enemiesInRadius = Physics.OverlapSphere(missileCollisions[collisionEvent].intersection, explositonRadius, baseClass.enemyLayer);

                for (int i = 0; i < enemiesInRadius.Length; i++)
                {
                    Enemy enemyToDamage = EnemySpawner.enemyTransformPairs[enemiesInRadius[i].transform];
                    EnemyDamage damageToApply = new EnemyDamage(enemyToDamage, baseClass.DamageValue, enemyToDamage.damageResistance);
                    TowerDefenseManager.EnqueueDamageData(damageToApply);

                    damageValue = baseClass.DamageValue;
                }
            }
            else
            {
                Enemy enemyToDamage = other.GetComponent<Enemy>();
                EnemyDamage damageToApply = new EnemyDamage(enemyToDamage, baseClass.DamageValue, enemyToDamage.damageResistance);
                TowerDefenseManager.EnqueueDamageData(damageToApply);

                damageValue = baseClass.DamageValue;
            }
        }
    }
}
