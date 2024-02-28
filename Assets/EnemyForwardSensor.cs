using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyForwardSensor : MonoBehaviour
{
    public TowerBehaviour obstacleObject = null;

    public Enemy enemyBase;

    private void Start()
    {
        enemyBase = transform.parent.GetComponent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle") && obstacleObject == null)
        {
            obstacleObject = other.gameObject.GetComponent<TowerBehaviour>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("Removed At YYY // Collider: " + other + " / " + other.gameObject + " / " + other.gameObject.tag.ToString());
        if ( other.gameObject.CompareTag("Obstacle"))// && obstacleObject != null)
        {
            //Debug.Log("Removed At XXX");
            obstacleObject = null;
        }
    }

}
