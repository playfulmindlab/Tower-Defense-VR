using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolCreatorMissileCollision : MonoBehaviour
{
    [SerializeField] private MissileDamage baseClass;
    [SerializeField] private ParticleSystem explosionSystem;
    [SerializeField] private ParticleSystem missileSystem;
    [SerializeField] private float explositonRadius;
    private List<ParticleCollisionEvent> missileCollisions;

    public float damageValue = 0;
    public GameObject poolPrefab;

    // Start is called before the first frame update
    void Start()
    {
        missileCollisions = new List<ParticleCollisionEvent>();
        Physics.IgnoreLayerCollision(2, 7);
    }

    private void OnParticleCollision(GameObject other)
    {
        missileSystem.GetCollisionEvents(other, missileCollisions);

        for (int collisionEvent = 0; collisionEvent < missileCollisions.Count; collisionEvent++)
        {
            explosionSystem.transform.position = missileCollisions[collisionEvent].intersection;
            explosionSystem.Play();

            Vector3 spawnPoint = missileCollisions[collisionEvent].intersection + (Vector3.down * missileSystem.main.startSizeYMultiplier / 2f);
            Instantiate(poolPrefab, spawnPoint, Quaternion.identity);
        }
    }
}

