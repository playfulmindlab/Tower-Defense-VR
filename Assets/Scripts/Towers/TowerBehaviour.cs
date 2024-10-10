using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ElementType { None, Fire, Ice, Electric }

public class TowerBehaviour : MonoBehaviour
{
    public LayerMask targetLayer;

    public Enemy target;
    public Transform towerPivot;
    public bool canFire = true;

    public bool followEnemy = true;

    public int towerCost = 100;
    [SerializeField] protected int health = 10;
    public int maxHealth = 10;
    public int shield = 10;

    public int damage;
    public float firerate;
    public float range;

    public float readjustSpeed = 3f;

    public Slider healthBar;
    public Slider shieldBar;
    public Outline outline;

    public GameObject upgradedTower;
    [SerializeField] PropManager propParent;

    protected IDamageMethod currentDamageMethodClass;
    protected float delay;
    private float healthDamageMod = 1;

    public List<Effect> activeEffects;
    [SerializeField] RawImage stunnedImage;

    public bool aliveOnSceneStart = false;

    //Outline towerOutline;
    public void ToggleOutline(bool isActive) 
    { 
        if (isActive) 
            outline.ChangeOutlineWidth(3); 
        else 
            outline.ChangeOutlineWidth(0); 
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentDamageMethodClass = GetComponent<IDamageMethod>();

        health = maxHealth;

        if (healthBar != null && shieldBar != null)
        {
            healthBar.maxValue = health;
            shieldBar.maxValue = shield;

            healthBar.value = health;
            shieldBar.value = shield;
        }

        if (currentDamageMethodClass == null)
        {
            Debug.LogError("Tower " + this.gameObject.name + " has no damage class attached!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, firerate);
        }

        if (GetComponentInChildren<Outline>())
        {
            outline = GetComponentInChildren<Outline>();
            outline.OutlineColor = Color.red;
            outline.ChangeOutlineWidth(0);
        }

        //Uncomment this if you want to test healing or damage to towers
        //health = 1;
        //Damage(0);

        delay = 1 / firerate;

        activeEffects = new List<Effect>();

        if (stunnedImage != null && stunnedImage.enabled == true) stunnedImage.enabled = false;

        if (aliveOnSceneStart)
        {
            StartCoroutine(AddAliveTowerToTowerManager());
        }
    }

    public void AssignPropParent(PropManager newPropParent)
    {
        propParent = newPropParent;
    }

    IEnumerator AddAliveTowerToTowerManager()
    {
        yield return new WaitForSeconds(0.2f);

        TowerDefenseManager.towersInGame.Add(this);
        gameObject.layer = 9;

        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.layer = 9;
        }

        gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
    }

    // Update is called once per frame
    public virtual void Tick()
    {
        if (healthBar != null && shieldBar != null)
        {
            healthBar.value = health;
            shieldBar.value = shield;
        }

        if (canFire == true)
        {
            currentDamageMethodClass.DamageTick(target);
        }

        if (followEnemy == true) { 
            if (target != null)
            {
                Vector3 posDifference = target.transform.position - transform.position;
                posDifference.y = 0;
                if (towerPivot != null)
                    towerPivot.transform.rotation = Quaternion.Slerp(towerPivot.transform.rotation, Quaternion.LookRotation(posDifference, Vector3.up), Time.deltaTime * readjustSpeed);
                    //towerPivot.transform.rotation = Quaternion.LookRotation(posDifference, Vector3.up);
            }
        }

        ActivateStatusEffects();
    }

    public void UpgradeTower(MiniMapTowerPlacement towerPlacement, PropManager oldProp)
    {
        GameManager.instance.LogNewEvent("PPO Upgrade", this.gameObject, transform.position, GameControlManager.instance.IsJumped);
        Debug.Log(this.gameObject.name + " /// " + upgradedTower.name);

        towerPlacement.UpgradeTower(this.gameObject, oldProp, upgradedTower);
    }

    public virtual void Damage(int damage)
    {
        if (shield > 0)
        {
            if (shield - damage < 0)
            {
                damage -= shield;
                shield = 0;
            }
            else
            {
                shield -= damage;
                damage = 0;
            }
        }

        health -= damage;
        AudioManager.instance.PlaySFXArray("TowerAttacked", transform.position);

        if (health <= 0 && this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
            TowerDie();
        }

        if (health <= maxHealth * 0.3f)
        {
            healthDamageMod = 0.5f;
            currentDamageMethodClass.UpdateDamage(this.damage * healthDamageMod);
        }
    }

    public virtual void TowerDie()
    {
        //TODO: wirte code for what happens when tower dies here
        AudioManager.instance.PlaySFXArray("TowerDestroyed", transform.position);
        //this.gameObject.SetActive(false);
        canFire = false;

        propParent.DeletePropAndTower();
        //TowerDefenseManager.EnqueueTowerToRemove(this);

        //Destroy(gameObject);
    }

    public void Heal(int healing)
    {
        health += healing;

        if (health < maxHealth)
            AudioManager.instance.PlaySFXArray("TowerHeal", transform.position);
        else if (health > maxHealth)
            health = maxHealth;

        if (health >= maxHealth * 0.3f)
        {
            healthDamageMod = 1.0f;
            currentDamageMethodClass.UpdateDamage(damage * healthDamageMod);
        }
    }

    protected void ActivateStatusEffects()
    {
        for (int i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i].GetEffectType() == EffectType.Stun)
            {
                activeEffects[i].expireTime -= Time.deltaTime;

                if (activeEffects[i].expireTime > 0)
                {
                    canFire = false;
                    ToggleStunImage(!canFire);
                }
                else
                {
                    canFire = true;
                    ToggleStunImage(!canFire);
                }
            }
        }

        activeEffects.RemoveAll(x => x.expireTime <= 0f);
    }

    void ToggleStunImage(bool toggle)
    {
        if (stunnedImage != null)
            stunnedImage.enabled = toggle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, range);
        if (target != null && towerPivot != null)
        {
            Gizmos.DrawLine(towerPivot.position, target.transform.position);
        }
    }
}
