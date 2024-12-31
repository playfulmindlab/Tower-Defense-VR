using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseBarricade : TowerBehaviour
{
    [SerializeField] Slider baseSlider;
    [SerializeField] Animator baseAttackedAnim;

    protected override void Start()
    {
        base.Start();
        //anim = GetComponent<Animator>();
        baseSlider.maxValue = shield;
    }

    public override void Tick()
    {
        if (healthBar != null && shieldBar != null)
        {
            healthBar.value = health;
            shieldBar.value = shield;

            baseSlider.value = shield;
        }
    }

    public override void Damage(int damage)
    {
        if (shield > 0)
        {
            baseAttackedAnim.SetTrigger("Reset");
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
        AudioManager.instance.PlaySFXArray("BaseAttacked", transform.position);

        if (health <= 0 && this.gameObject.activeSelf)
        {
            DataEvent newEvent = new DataEvent("Player Death", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
            EventManager.instance.RecordNewEvent(newEvent);

            this.gameObject.SetActive(false);
            TowerDie();
        }
    }

    public override void TowerDie()
    {
        AudioManager.instance.PlaySFXArray("BaseDestroyed", transform.position);
        TowerDefenseManager.BeginGameOverSequence();
        //base.TowerDie();
    }
}
