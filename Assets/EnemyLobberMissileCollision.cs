using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLobberMissileCollision : MissileDamage
{
    [SerializeField] private Enemy baseClass;
    [SerializeField] private ParticleSystem explosionSystem;
    private List<ParticleCollisionEvent> missileCollisions;

    // Start is called before the first frame update
    void Start()
    {
        missileCollisions = new List<ParticleCollisionEvent>();
        Physics.IgnoreLayerCollision(0, 10);
    }

    public override void ActivateGun(bool activeState) { }

    public override void DamageTick(Enemy target) { }

    private void OnParticleCollision(GameObject other)
    {
        missileSystem.GetCollisionEvents(other, missileCollisions);
        Debug.Log("OTHER: " + other.name);

        for (int collisionEvent = 0; collisionEvent < missileCollisions.Count; collisionEvent++)
        {
            explosionSystem.transform.position = missileCollisions[collisionEvent].intersection;
            explosionSystem.Play();

            TowerBehaviour towerToDamage = other.GetComponent<TowerBehaviour>();
            Debug.Log("ATK TOWER: " + towerToDamage);
            if (towerToDamage != null)
            {
                towerToDamage.Damage(baseClass.attack);
            }

            //Collider[] enemiesInRadius = Physics.OverlapSphere(missileCollisions[collisionEvent].intersection, explosionRadius, 1 << LayerMask.NameToLayer("Tower"));

            /*for (int i = 0; i < enemiesInRadius.Length; i++)
            {
                //Debug.Log("Collider: " + enemiesInRadius[i] + " Layer: " + enemiesInRadius[i].gameObject.layer);
                Enemy enemyToDamage = EnemySpawner.enemyTransformPairs[enemiesInRadius[i].transform];
                EnemyDamage damageToApply = new EnemyDamage(enemyToDamage, baseClass.DamageValue, enemyToDamage.GetResistanceModifier(baseClass.GetAttackType));
                TowerDefenseManager.EnqueueDamageData(damageToApply);
            }*/
        }
    }
}
