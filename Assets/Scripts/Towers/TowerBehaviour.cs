using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ElementType { None, Fire, Ice, Electric }

public class TowerBehaviour : MonoBehaviour
{
    public LayerMask enemiesLayer;

    public Enemy target;
    public Transform towerPivot;

    public int towerCost = 100;
    int health = 10;
    public int maxHealth = 10;
    public int shield = 10;

    public float damage;
    public float firerate;
    public float range;
    public ElementType damageType;

    public Slider healthBar;
    public Slider shieldBar;

    private IDamageMethod currentDamageMethodClass;
    private float delay;
    private float healthDamageMod = 1;

    // Start is called before the first frame update
    void Start()
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

        delay = 1 / firerate;
    }

    // Update is called once per frame
    public void Tick()
    {
        if (healthBar != null && shieldBar != null)
        {
            healthBar.value = health;
            shieldBar.value = shield;
        }

        currentDamageMethodClass.DamageTick(target);

        if (target != null)
        {
            Vector3 posDifference = target.transform.position - transform.position;
            posDifference.y = 0;
            if (towerPivot != null)
                towerPivot.transform.rotation = Quaternion.LookRotation(posDifference, Vector3.up);
        }
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
            TowerDie();

        if (health <= maxHealth * 0.3f)
        {
            healthDamageMod = 0.5f;
            currentDamageMethodClass.UpdateDamage(damage * healthDamageMod);
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
