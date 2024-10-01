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
        Physics.IgnoreLayerCollision(0, 6);
    }

    private void OnParticleCollision(GameObject other)
    {
        //Debug.Log("Layer Collision: " + other.layer);
        missileSystem.GetCollisionEvents(other, missileCollisions);

        //Debug.Log("Collision Events Count: " + missileCollisions.Count + " / " + other.name);

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
                    EnemyDamage damageToApply = new EnemyDamage(enemyToDamage, baseClass.DamageValue, enemyToDamage.GetResistanceModifier(baseClass.GetAttackType));
                    TowerDefenseManager.EnqueueDamageData(damageToApply);

                    damageValue = baseClass.DamageValue;
                }
            }
            else
            {
                Enemy enemyToDamage = other.GetComponent<Enemy>();
                damageValue = baseClass.DamageValue;
                if (enemyToDamage != null)
                {
                    EnemyDamage damageToApply = new EnemyDamage(enemyToDamage, baseClass.DamageValue, enemyToDamage.GetResistanceModifier(baseClass.GetAttackType));
                    TowerDefenseManager.EnqueueDamageData(damageToApply);

                    if (baseClass.GetAttackType != ElementType.None)
                    {
                        ApplyEffectBasedOnDamage(baseClass.GetAttackType, enemyToDamage);
                    }
                }
            }
        }
    }

    void ApplyEffectBasedOnDamage(ElementType element, Enemy enemy)
    {
        switch (element)
        {
            case ElementType.Ice:
                Effect slowEffect = new Effect(gameObject.name + " - Slow", 0.5f, enemy.Speed, 3f);
                AppliedEffect effect = new AppliedEffect(enemy, slowEffect);
                AudioManager.instance.PlaySFXArray("FrozenStatus", enemy.transform.position);
                TowerDefenseManager.EnqueueEffectToApply(effect);
                break;
            default:
                Debug.Log("Invalid Element Type Detected - No Effect Applied");
                break;
        }
    }
}
