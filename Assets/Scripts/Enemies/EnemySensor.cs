using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySensor : MonoBehaviour
{
    [SerializeField] bool attackOnlyObstacles;

    Enemy enemyBase;

    private void Start()
    {
        enemyBase = transform.parent.GetComponent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attackOnlyObstacles && other.gameObject.CompareTag("Tower"))
        {
            if (other.gameObject.GetComponent<TowerBehaviour>() != null)
            {
                enemyBase.ChangeTowerTarget(other.gameObject.GetComponent<TowerBehaviour>());
            }
        }
        if (other.gameObject.CompareTag("AttackObstacle") || other.gameObject.CompareTag("BaseObstacle"))
        {
            if (other.gameObject.GetComponent<TowerBehaviour>() != null)
            {
                enemyBase.ChangeTowerTarget(other.gameObject.GetComponent<TowerBehaviour>());

                if (other.gameObject.CompareTag("BaseObstacle"))
                {
                    DataEvent newEvent = new DataEvent(enemyBase.gameObject.name + " Reached Home Base", "N/A", "N/A", GameControlManager.instance.IsJumped.ToString());
                    EventManager.instance.RecordNewEvent(newEvent);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("AttackObstacle") || 
            other.gameObject.CompareTag("BaseObstacle") ||
            other.gameObject.CompareTag("Tower"))
        {
            enemyBase.ChangeTowerTarget(null);
        }
    }

}
