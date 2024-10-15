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
                if (healthText != null)
                    healthText.text = health + "/" + maxHealth;
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
    EnemySensor towerSensor;
    [SerializeField] TowerBehaviour attackingTower;

    public float damageResistance = 1f;
    public DamageResistance[] damageResistances = new DamageResistance[0];
    public List<Effect> activeEffects;

    public Node[] currNodePath;
    public int[] currNodeIndices;
    public int reward = 10;
    public int id;
    public int nodeIndex;
    int indexIndex;

    Dictionary<ElementType, float> damResistancesDict = new Dictionary<ElementType, float>();

    [SerializeField] AudioClip deathSound;
    [SerializeField] Slider healthBar;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI speedText;

    Animator anim;
    float origSpeed;
    protected float attackDelay = 1f;
    bool speedAffected = false;

    bool reachedEnd = false;

    float shockDamage = 5;
    float shockDamageMax = 5;

    private void Start()
    {
        gameObject.name = gameObject.name.Replace("(Clone)", " ");
        int randID = Random.Range(0, 10000);
        gameObject.name += randID.ToString("D4"); 
        
        shockDamage = shockDamageMax;
    }

    public void Init()
    {
        Health = maxHealth;
        activeEffects = new List<Effect>();
        reachedEnd = false;

        damResistancesDict.Clear();
        foreach (DamageResistance damRes in damageResistances)
        {
            damResistancesDict.Add(damRes.resistanceType, damRes.resistanceModifier);
        }

        transform.position = TowerDefenseManager.nodePositions[0];
        Speed = initialSpeed + ((TowerDefenseManager.waveCount - 1) * speedIncreasePerWave);
        nodeIndex = 0;
        indexIndex = 0;

        origSpeed = Speed;

        attackDelay = 1 / attackRate;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = health;
        }

        anim = transform.GetComponentInChildren<Animator>();

        if (transform.GetComponentsInChildren<EnemySensor>().Length > 0)
        {
            towerSensor = transform.GetComponentsInChildren<EnemySensor>()[0];
        }

        GameManager.instance.LogNewEvent("Enemy Spawn", gameObject, transform.position, GameControlManager.instance.IsJumped);
        //DataEvent newEvent = new DataEvent("Enemy Spawn", gameObject, transform.position, GameControlManager.instance.IsJumped);
        //EventManager.instance.RecordNewEvent(newEvent);
    }

    public float GetResistanceModifier(ElementType attackType)
    {
        //Debug.Log("COUNT: " + damResistancesDict.Count + " // CONTAINED?: " + damResistancesDict.ContainsKey(attackType));
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
                //anim.SetInteger("AttackIndex", Random.Range(0, 3));
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
            Vector3 positionToRotateTowards = newTower.transform.position;
            positionToRotateTowards.y = transform.position.y;
            Vector3 newDir = transform.position - positionToRotateTowards;
            transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);

            //Vector3 targetPos = newTower.transform.position;
            //targetPos.y = transform.position.y;
            //transform.LookAt(targetPos, Vector3.up);

            Speed = 0f;
        }
        else
        {
            if (anim != null)
            {
                anim.SetBool("Stopped", false);
            }
            //Vector3 positionToRotateTowards = currNodePath[nodeIndex].transform.position;
            Vector3 positionToRotateTowards = currNodePath[indexIndex].transform.position;
            Vector3 newDir = transform.position - positionToRotateTowards;
            transform.rotation = Quaternion.LookRotation(newDir, Vector3.up);

            Speed = origSpeed;
        }
    }

    public virtual void Tick()
    {
        //if it's reached the end, remove this enemy from the game
        if (reachedEnd)
        {
            RemoveFromGame();
            return;
        }

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
                    shockDamage -= activeEffects[i].damage;
                    if (shockDamage > 0f)
                    {
                        TowerDefenseManager.EnqueueDamageData(new EnemyDamage(this, activeEffects[i].damage, GetResistanceModifier(activeEffects[i].element)));
                        return;
                    }
                    if (activeEffects[i].stopExpireTime <= 0f)
                    {
                        Effect shockEffect = activeEffects[i];
                        if (Speed == 0) //if speed = 0, switch to moving
                        {
                            Speed = shockEffect.origSpeed;
                            shockEffect.stopExpireTime = shockEffect.stopIntervalTime;
                            speedAffected = false;
                            shockDamage = shockDamageMax;
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

    //public Node GetNextNode()
    //    nodeIndex++;
    //    return currNodePath[nodeIndex];
    //}

    public int GetNextIndex()
    {
        //Debug.Log("ENEMY: " + EnemySpawner.enemiesInGame[g].gameObject.name + " VS " + gameObject.name);
        Debug.Log("Node Count: " + nodeIndex + " IndexCount: " + indexIndex + " Curr Node: " + currNodeIndices[indexIndex] + " out of " + currNodeIndices.Length);

        //if the NodeIndex is equal to the total length of the current node indices, it means the enemy has reached
        //the end of the path, and itshould be removed
        if (indexIndex < currNodeIndices.Length - 1)
        {
            indexIndex++;
            nodeIndex = currNodeIndices[indexIndex];
        }
        //else
        //{
        //    GameManager.instance.LogNewEvent("Enemy Finished", this.gameObject, transform.position, GameControlManager.instance.IsJumped);
        //    TowerDefenseManager.EnqueueEnemyToRemove(this);
        //}

        return nodeIndex;
    }

    public void ToggleEndOfPath()
    {
        reachedEnd = true;
    }

    public void RemoveFromGame()
    {
        string healthRemaining = Health.ToString() + " / " + maxHealth.ToString();
        GameManager.instance.LogNewEvent("Enemy Finished", this.gameObject, transform.position, GameControlManager.instance.IsJumped,
            healthRemaining);
        TowerDefenseManager.EnqueueEnemyToRemove(this);
    }

    public void UpdateNodeIndex()
    {

    }
}
