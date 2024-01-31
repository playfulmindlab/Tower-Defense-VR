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

    public int towerCost = 100;
    [SerializeField] protected int health = 10;
    public int maxHealth = 10;
    public int shield = 10;

    public int damage;
    public float firerate;
    public float range;

    public Slider healthBar;
    public Slider shieldBar;

    public GameObject upgradedTower;

    protected IDamageMethod currentDamageMethodClass;
    protected float delay;
    private float healthDamageMod = 1;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentDamageMethodClass = GetComponent<IDamageMethod>();

        health = maxHealth;

        if (healthBar != null && shieldBar != null)
        {
            healthBar.maxValue = health;
            shieldBar.maxValue = shield;
        }

        if (currentDamageMethodClass == null)
        {
            Debug.LogError("Tower " + this.gameObject.name + " has no damage class attached!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, firerate);
        }

        //Uncomment this if you want to test healing or damage to towers
        health = 1;
        Damage(0);

        delay = 1 / firerate;
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

            if (target != null)
            {
                Vector3 posDifference = target.transform.position - transform.position;
                posDifference.y = 0;
                if (towerPivot != null)
                    towerPivot.transform.rotation = Quaternion.LookRotation(posDifference, Vector3.up);
            }
        }
    }

    public void UpgradeTower()
    {
        TowerPlacementVR towerPlacement = GameObject.FindWithTag("Player").GetComponent<TowerPlacementVR>();

        towerPlacement.UpgradeTower(this.gameObject, upgradedTower);
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
            TowerDie();

        if (health <= maxHealth * 0.3f)
        {
            healthDamageMod = 0.5f;
            Debug.Log("Damage Pre " + GetComponent<Damage>().DamageValue + " // " + (this.damage * healthDamageMod));
            currentDamageMethodClass.UpdateDamage(this.damage * healthDamageMod);
            Debug.Log("Damage Post " + GetComponent<Damage>().DamageValue);
        }
    }

    public void TowerDie()
    {
        //TODO: wirte code for what happens when tower dies here
        Destroy(gameObject);
    }

    public void Heal(int healing)
    {
        health += healing;

        if (health > maxHealth)
            health = maxHealth;

        if (health >= maxHealth * 0.3f)
        {
            healthDamageMod = 1.0f;
            currentDamageMethodClass.UpdateDamage(damage * healthDamageMod);
        }
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
