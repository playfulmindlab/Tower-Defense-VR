using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform root;
    public float maxHealth;
    public float health;
    public float speed;
    public List<Effect> activeEffects;
    public float damageResistance = 1f;
    public int reward = 10;
    public int id;
    public int nodeIndex;

    public void Init()
    {
        health = maxHealth;
        activeEffects = new List<Effect>();
        transform.position = TowerDefenseManager.nodePositions[0];
        speed = TowerDefenseManager.waveCount * 2;
        nodeIndex = 0;
    }

    public void Tick()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i].expireTime > 0f)
            {
                if (activeEffects[i].damageDelay > 0f)
                {
                    activeEffects[i].damageDelay -= Time.deltaTime;
                }
                else
                {
                    TowerDefenseManager.EnqueueDamageData(new EnemyDamage(this, activeEffects[i].damage, 1f));
                    activeEffects[i].damageDelay = 1f / activeEffects[i].damageRate;
                }
                activeEffects[i].expireTime -= Time.deltaTime;
            }
        }
        activeEffects.RemoveAll(x => x.expireTime <= 0f);
    }
}
