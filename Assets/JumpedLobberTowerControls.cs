using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpedLobberTowerControls : JumpedTowerControls
{
    [SerializeField] LineRenderer trajectoryLine;
    [SerializeField] ParticleSystem missileSystem;
    [SerializeField] ParticleSystem.MainModule missileSystemMain;
    [SerializeField, Min(3)] int lineSegments = 20;

    [SerializeField] Transform headTransform;
    [SerializeField] Transform tipTransform;

    float flightTime = 3f;

    GameObject targetDecal;
    Vector3 targetPoint = Vector3.zero;
    RaycastHit hit;
    float attackRange = 60f;


    // Start is called before the first frame update
    void Start()
    {
        missileSystemMain = missileSystem.main;
        targetDecal = GameObject.Find("LobberTargetDecal");
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(towerCamera.transform.position, towerCamera.transform.forward, out hit, attackRange))
        {
            targetDecal.transform.position = hit.point;
            targetPoint = hit.point;

            RenderTrajectory();
        }

    }

    void ChangeCameraRotation(Vector3 newCoords, float dampVal)
    {
        towerCamera.transform.localRotation = Quaternion.Slerp(towerCamera.transform.localRotation, Quaternion.Euler(newCoords), Time.deltaTime * dampVal);
    }

    public override void RotateGun(Vector2 balanceBoardCoords, float dampVal = 5f)
    {
        ChangeCameraRotation(new Vector3((balanceBoardCoords.y - towerRotation) * 2f, -balanceBoardCoords.x * 2, 0f), dampVal);

        Quaternion rot = Quaternion.Euler(new Vector3(0f, -balanceBoardCoords.x * 2, 0f));
        towerHead.localRotation = Quaternion.Slerp(towerHead.localRotation, rot, Time.deltaTime * dampVal);
    }

    void RenderTrajectory()
    {
        Debug.Log("RenderTrajectory Gothru");
        Vector3 startPoint = tipTransform.position;
        Vector3 endPoint = targetPoint;
        Vector3[] positions = new Vector3[lineSegments + 1];
        trajectoryLine.positionCount = lineSegments;

        Debug.Log("Start: " + startPoint + " / End: " + endPoint);
        //Debug.Log("Seg Count: " + positions.Length);

        float distance = Vector3.Distance(endPoint, startPoint);
        float gravity = missileSystemMain.gravityModifierMultiplier * 9.81f;

        float projectileSpeed = Mathf.Sqrt(distance * gravity);
        missileSystemMain.startSpeed = projectileSpeed;

        float xVal = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            float arcPart = (float)i / lineSegments;
            xVal = arcPart * distance;
            Debug.Log("ArcPart: " + arcPart + " / Distance: " + distance + " / XValue: " + xVal + " / Loop: " + i + " / Distance: " + distance);

            float yVal = xVal - ((gravity * xVal * xVal) / (projectileSpeed * projectileSpeed));

            positions[i] = tipTransform.position + (xVal * towerHead.forward) + (yVal * Vector3.up);
        }

        trajectoryLine.SetPositions(positions);
    }

    public override void EndTowerJump()
    {
        //Quaternion rot = Quaternion.Euler(Vector3.zero);
        //towerHead.localEulerAngles = 
    }
}
