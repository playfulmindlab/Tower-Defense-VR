using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLobberMissileCollision : Damage
{
    //public LayerMask towerLayer;
    [SerializeField] protected ParticleSystem missileSystem;
    //[SerializeField] protected Transform towerHead;
    [SerializeField] string shotHitSFXName = "TowerBasicShot";

    protected ParticleSystem.MainModule missileSystemMain;
    //protected AudioSource audioSource;



    [SerializeField] private Enemy baseClass;
    [SerializeField] private ParticleSystem explosionSystem;
    private List<ParticleCollisionEvent> missileCollisions;

    public override void Init(float damage, float firerate)
    {
        //audioSource = GetComponent<AudioSource>();
        missileSystemMain = missileSystem.main;
        missileSystemMain.duration = 1 / firerate;

        //missileSystemMain.startSpeed = projectileSpeed;
        base.Init(damage, firerate);
    }

    // Start is called before the first frame update
    void Start()
    {
        missileCollisions = new List<ParticleCollisionEvent>();
        Physics.IgnoreLayerCollision(0, 10);
    }

    //public override void ActivateGun(bool activeState) { }

    public override void DamageTick(Enemy target) { }

    private void OnParticleCollision(GameObject other)
    {
        missileSystem.GetCollisionEvents(other, missileCollisions);
        Debug.Log("OTHER: " + other.name);

        AudioManager.instance.PlaySFXArray(shotHitSFXName, other.transform.position);

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

            //Note: this section of code is meant for damage dealt in a radius around an exploded missile - the lobber currently doesn't use
            //this at all right now, but the code is here easy to implement it if we so choose

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
