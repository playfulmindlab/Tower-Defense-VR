using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowVRCamera : MonoBehaviour
{
    [SerializeField] Transform playerCamera;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerCamera.position.x, transform.position.y, playerCamera.position.z);
        transform.rotation = Quaternion.Euler(new Vector3(0f, playerCamera.rotation.eulerAngles.y, 0f));
    }
}
