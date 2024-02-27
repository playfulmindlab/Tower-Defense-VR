using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct DamageResistance
{
    public ElementType resistanceType;
    public float resistanceModifier;
}

public class Enemy : MonoBehaviour
{
    public Transform root;
    public float maxHealth;

    [SerializeField] float health;
    public float Health
    {
        get { return health; }
        set
        {
            health = value;
            if (healthBar != null)
            {
                healthBar.value = health;
            }
        }
    }

    public float speed;
    public float Speed
    {
        get { return speed; }
        set {
            speed = value;
            if (speedText != null)
                speedText.text = "Speed: " + Speed.ToString();
        }
    }

    public int attack = 3;
    public float attackRate = 1f;

    public List<Effect> activeEffects;
    public float damageResistance = 1f;
    public DamageResistance[] damageResistances = new DamageResistance[0];
    public int reward = 10;
    public int id;
    public int nodeIndex;

    Dictionary<ElementType, float> damResistancesDict = new Dictionary<ElementType, float>();

    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI speedText;

    public void Init()
    {
        health = maxHealth;
        activeEffects = new List<Effect>();

        damResistancesDict.Clear();
        foreach (DamageResistance damRes in damageResistances)
        {
            damResistancesDict.Add(damRes.resistanceType, damRes.resistanceModifier);
        }

        transform.position = TowerDefenseManager.nodePositions[0];
        Speed = 1 + (TowerDefenseManager.waveCount * 0.7f);
        nodeIndex = 0;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }
    }

    public float GetResistanceModifier(ElementType attackType)
    {
        Debug.Log("COUNT: " + damResistancesDict.Count + " // CONTAINED?: " + damResistancesDict.ContainsKey(attackType));
        if (damResistancesDict.Count > 0 && damResistancesDict.ContainsKey(attackType))
            return damResistancesDict[attackType];

        return 1f;
    }

    public virtual void Attack(TowerBehaviour attackedObject)
    {
        attackedObject.Damage(attack);
    }

    public void Tick()
    {
        //Attack Obstacle


        //Effects Activate
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i].GetEffectType() == EffectType.Damage)
            {
                if (activeEffects[i].expireTime > 0f)
                {
                    if (activeEffects[i].damageDelay > 0f)
                    {
                        activeEffects[i].damageDelay -= Time.deltaTime;
                    }
                    else
                    {
                        TowerDefenseManager.EnqueueDamageData(new EnemyDamage(this, activeEffects[i].damage, GetResistanceModifier(activeEffects[i].element)));
                        activeEffects[i].damageDelay = 1f / activeEffects[i].damageRate;
                    }
                    activeEffects[i].expireTime -= Time.deltaTime;
                }
            }
            else if (activeEffects[i].GetEffectType() == EffectType.Slow)
            {
                activeEffects[i].expireTime -= Time.deltaTime;
                if (activeEffects[i].expireTime > 0)
                {
                    Speed = activeEffects[i].origSpeed * activeEffects[i].slowAmount;
                    Debug.Log("Step 1: " + speed + " // " + activeEffects[i].origSpeed + " x " + activeEffects[i].slowAmount);
                }
                else
                {
                    Speed = activeEffects[i].origSpeed;
                    Debug.Log("Step 2: " + speed);
                }
            }
            else if (activeEffects[i].GetEffectType() == EffectType.Shock)
            {
                activeEffects[i].expireTime -= Time.deltaTime;
                activeEffects[i].stopExpireTime -= Time.deltaTime;

                if (activeEffects[i].expireTime > 0) {
                    if (activeEffects[i].stopExpireTime <= 0f)
                    {
                        Effect shockEffect = activeEffects[i];

                        if (Speed == 0) //if speed = 0, switch to moving
                        {
                            Speed = shockEffect.origSpeed;
                            shockEffect.stopExpireTime = shockEffect.stopIntervalTime; 
                        }
                        else //otherwise, enemy has NOT stopped and needs to be!
                        {
                            Speed = 0f;
                            shockEffect.stopExpireTime = shockEffect.resumeIntervalTime ;
                        }
                    }
                }
                else
                {
                    Speed = activeEffects[i].origSpeed;
                }
            }
        }
        activeEffects.RemoveAll(x => x.expireTime <= 0f);
    }
}
