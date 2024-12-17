using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    Camera cam;
    float recheckCameraTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        transform.LookAt(cam.transform.position, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        if (cam != null)
            transform.LookAt(cam.transform.position, Vector3.up);
        /*
                recheckCameraTime -= Time.deltaTime;
                if (recheckCameraTime <= 0)
                {
                    cam = Camera.main;
                    recheckCameraTime = 3f;
                }
        */
    }
}

/*public class FacePlayer : MonoBehaviour
{
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;


        Vector3 lookDir = transform.position - cam.transform.position;
        float radians = Mathf.Atan2(lookDir.x, lookDir.z);
        float degrees = radians * Mathf.Rad2Deg;

        //float str = Mathf.Min(movementStrength * Time.deltaTime, 1);
        Quaternion targetRotation = Quaternion.Euler(0, degrees, 0);
        transform.rotation = targetRotation;

        //Quaternion ySpin = Quaternion.Euler(0, yDegrees, 0);
    }

}*/

