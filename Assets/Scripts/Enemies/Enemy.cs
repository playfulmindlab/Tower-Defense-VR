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
                speedText.text = "Speed: " + Speed.ToString("F2");
        }
    }
    [SerializeField] float initialSpeed = 1f;
    [SerializeField] float speedIncreasePerWave = 0.5f;

    public int attack = 3;
    public float attackRate = 1f;
    EnemyForwardSensor obstacleSensor;
    [SerializeField] TowerBehaviour attackingTower;

    public float damageResistance = 1f;
    public DamageResistance[] damageResistances = new DamageResistance[0];
    public List<Effect> activeEffects;

    public int reward = 10;
    public int id;
    public int nodeIndex;

    Dictionary<ElementType, float> damResistancesDict = new Dictionary<ElementType, float>();

    [SerializeField] AudioClip deathSound;
    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI speedText;

    Animator anim;
    float origSpeed;
    float attackDelay = 1f;
    bool speedAffected = false;

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
        Speed = initialSpeed + ((TowerDefenseManager.waveCount - 1) * speedIncreasePerWave);
        nodeIndex = 0;

        origSpeed = Speed;

        attackDelay = 1 / attackRate;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        anim = transform.GetComponentInChildren<Animator>();

        if (transform.GetComponentsInChildren<EnemyForwardSensor>().Length > 0)
        {
            obstacleSensor = transform.GetComponentsInChildren<EnemyForwardSensor>()[0];
        }

        GameManager.instance.LogNewEvent("Enemy Spawn", gameObject, transform.position, GameControlManager.instance.IsJumped);
        //DataEvent newEvent = new DataEvent("Enemy Spawn", gameObject, transform.position, GameControlManager.instance.IsJumped);
        //EventManager.instance.RecordNewEvent(newEvent);
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
        if (attackedObject != null)
        {
            if (anim != null)
            {
                anim.SetInteger("AttackIndex", Random.Range(0, 3));
                anim.SetTrigger("Attack");
            }

            //AudioManager.instance.PlaySFXRandom("EnemyAttack", transform.position, 3);
            attackedObject.Damage(attack);
        }
    }

    public void ChangeTowerTarget(TowerBehaviour newTower)
    {
        attackingTower = newTower;

        if (attackingTower != null)
        {
            if (anim != null)
            {
                anim.SetBool("Stopped", true);
            }
            Speed = 0f;
        }
        else
        {
            if (anim != null)
            {
                anim.SetBool("Stopped", false);
            }
            Speed = origSpeed;
        }
    }

    public void Tick()
    {
        //Attack Obstacle
        if (attackingTower != null)
        {
            attackDelay -= Time.deltaTime;
            if (attackDelay <= 0)
            {
                Attack(attackingTower);
                attackDelay = 1 / attackRate;
            }
        }
        else if (speed == 0 && speedAffected == false && attackingTower == null)
        {
            ChangeTowerTarget(null);
            //Speed = origSpeed;
        }

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
                    speedAffected = false;
                }
                else
                {
                    Speed = activeEffects[i].origSpeed;
                    Debug.Log("Step 2: " + speed);
                    speedAffected = true;
                }
            }
            else if (activeEffects[i].GetEffectType() == EffectType.Shock)
            {
                activeEffects[i].expireTime -= Time.deltaTime;
                activeEffects[i].stopExpireTime -= Time.deltaTime;

                if (activeEffects[i].expireTime > 0) {
                    //Speed = 0f;
                    if (activeEffects[i].stopExpireTime <= 0f)
                    {
                        Effect shockEffect = activeEffects[i];

                        if (Speed == 0) //if speed = 0, switch to moving
                        {
                            Speed = shockEffect.origSpeed;
                            shockEffect.stopExpireTime = shockEffect.stopIntervalTime;
                            speedAffected = false;
                        }
                        else //otherwise, enemy has NOT stopped and needs to be!
                        
                        {
                            Speed = 0f;
                            shockEffect.stopExpireTime = shockEffect.resumeIntervalTime ;
                            speedAffected = true;
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
