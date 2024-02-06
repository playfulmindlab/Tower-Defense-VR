using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemHolderScript : MonoBehaviour
{
    [SerializeField] GameObject itemHolder;
    [SerializeField] GameObject heldItem;

    [SerializeField] public InputActionProperty rTriggerButton;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (rTriggerButton.action.WasPerformedThisFrame())
        {
            Debug.Log("Out trigger x");
        }

        if (rTriggerButton.action.WasPerformedThisFrame() && heldItem != null)
        {
            Debug.Log("Out trigger y");
            heldItem.GetComponent<ItemScript>().ThrowItem();
            itemHolder.transform.GetChild(0);
            heldItem.transform.parent = null;
            heldItem = null;
        }
    }

    public void AttachItemToItemHolder(GameObject newItem)
    {
        GameObject spawnedObject = Instantiate(newItem, itemHolder.transform.position, Quaternion.identity);
        spawnedObject.transform.parent = itemHolder.transform;
        heldItem = spawnedObject;

        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;
    }
}
