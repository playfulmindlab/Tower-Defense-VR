using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] float throwingSpeed = 10f;
    [SerializeField] protected float radius;

    bool isThrown = false;

    // Start is called before the first frame update
    void Start()
    {
        throwingSpeed *= 0.1f;
        //Destroy(this.gameObject, 6f);
    }

    // Update is called once per frame
    void Update()
    {
        if (isThrown)
        {
            transform.position += transform.forward * throwingSpeed;
        }
    }

    public void ThrowItem()
    {
        isThrown = true;
        Destroy(this.gameObject, 6f);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnItemUse();
    }

    protected virtual void OnItemUse()
    {
        Destroy(this.gameObject);
    }
}
