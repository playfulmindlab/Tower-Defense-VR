using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyForwardSensor : MonoBehaviour
{
    public Enemy enemyBase;

    private void Start()
    {
        enemyBase = transform.parent.GetComponent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AttackObstacle") || other.gameObject.CompareTag("BaseObstacle"))// && obstacleObject == null)
        {
            if (other.gameObject.GetComponent<TowerBehaviour>() != null)
                enemyBase.ChangeTowerTarget(other.gameObject.GetComponent<TowerBehaviour>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Removed At YYY // Collider: " + other + " / " + other.gameObject + " / " + other.gameObject.tag.ToString());
        if (other.gameObject.CompareTag("AttackObstacle") || other.gameObject.CompareTag("BaseObstacle"))// && obstacleObject != null)
        {
            //Debug.Log("Removed At XXX");
            enemyBase.ChangeTowerTarget(null);
        }
    }

}
