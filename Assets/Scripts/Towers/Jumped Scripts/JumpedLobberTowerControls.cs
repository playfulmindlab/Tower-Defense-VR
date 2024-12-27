using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class JumpedLobberTowerControls : JumpedTowerControls
{
    [SerializeField] LineRenderer trajectoryLine;
    [SerializeField] ParticleSystem missileSystem;
    [SerializeField, Min(3)] int lineSegments = 20;
    [SerializeField] Transform tipTransform;

    [SerializeField] float cameraHeightMod = 3f;
    [SerializeField] float cameraAngleMod = 30f;

    GameObject targetDecal;
    Vector3 targetPoint = Vector3.zero;
    RaycastHit hit;
    float attackRange = 60f;
    ParticleSystem.MainModule missileSystemMain;

    float projectileSpeed = 0f;
    public float ProjectileSpeed { get { return projectileSpeed; } }

    float fireateTime = 3f;

    // Start is called before the first frame update
    void Start()
    {
        missileSystemMain = missileSystem.main;
        targetDecal = GameObject.Find("LobberTargetDecal");
        towerCamera.transform.localPosition = Vector3.up * cameraHeightMod;

        if (trajectoryLine.enabled)
            trajectoryLine.enabled = false;

        //fireateTime = gameObject.GetComponent<TowerBehaviour>().firerate;
        var em = missileSystem.emission;
        em.rateOverTime = gameObject.GetComponent<TowerBehaviour>().firerate;
    }

    // Update is called once per frame
    void Update()
    {
        //fireateTime -= Time.deltaTime;

        if (Physics.Raycast(towerCamera.transform.position, towerCamera.transform.forward, out hit, attackRange, 1 << 12))
        {
            targetDecal.transform.position = hit.point;
            targetPoint = hit.point;
            //targetDecal.transform.position = hit.point;

            //trajectoryLine.enabled = jumpStatus;
            //targetDecal.SetActive(jumpStatus);

            RenderTrajectory();
        }
    }

    void ChangeCameraRotation(Vector3 newCoords, float dampVal)
    {
        newCoords += Vector3.right * cameraAngleMod;
        towerCamera.transform.localRotation = Quaternion.Slerp(towerCamera.transform.localRotation, Quaternion.Euler(newCoords), Time.deltaTime * dampVal);
    }

    public override void RotateGun(Vector2 balanceBoardCoords, float dampVal = 5f)
    {
        ChangeCameraRotation(new Vector3((balanceBoardCoords.y - towerRotation) * 2f, balanceBoardCoords.x * 2, 0f), dampVal);

        Quaternion rot = Quaternion.Euler(new Vector3(0f, balanceBoardCoords.x * 2, 0f));
        towerHead.localRotation = Quaternion.Slerp(towerHead.localRotation, rot, Time.deltaTime * dampVal);
    }

    void RenderTrajectory()
    {
        Vector3 startPoint = tipTransform.position;
        Vector3 endPoint = targetPoint;
        Vector3[] positions = new Vector3[lineSegments + 1];
        trajectoryLine.positionCount = lineSegments;

        //Debug.Log("Start: " + startPoint + " / End: " + endPoint);

        float distance = Vector3.Distance(endPoint, startPoint);
        float gravity = missileSystemMain.gravityModifierMultiplier * 9.81f;

        projectileSpeed = Mathf.Sqrt(distance * gravity);
        missileSystemMain.startSpeed = projectileSpeed;

        float xVal = 0;

        for (int i = 0; i < positions.Length; i++)
        {
            float arcPart = (float)i / lineSegments;
            xVal = arcPart * distance;
            //Debug.Log("ArcPart: " + arcPart + " / Distance: " + distance + " / XValue: " + xVal + " / Loop: " + i + " / Distance: " + distance);

            float yVal = xVal - ((gravity * xVal * xVal) / (projectileSpeed * projectileSpeed));

            positions[i] = tipTransform.position + (xVal * towerHead.forward) + (yVal * Vector3.up);
        }

        trajectoryLine.SetPositions(positions);

    }

    public override void SetCamera(bool camActive)
    {
        base.SetCamera(camActive);
        trajectoryLine.enabled = camActive;
        targetDecal.GetComponent<DecalProjector>().enabled = camActive;//SetActive(camActive);
    }

    public override void EndTowerJump()
    {
        Debug.Log("GOTHRU EndTowerJump()");
        base.EndTowerJump();
        //Quaternion rot = Quaternion.Euler(Vector3.zero);
        //towerHead.localEulerAngles = 
        trajectoryLine.enabled = false;
        targetDecal.GetComponent<DecalProjector>().enabled = false;
        //targetDecal.SetActive(false);
        ForceGun(false);
    }

    public override void SetGunFire()
    {
        //Debug.Log("JUMP STAT 1: " + jumpStatus + " // " + trajectoryLine.enabled + " / " + targetDecal.activeSelf);
        base.SetGunFire();
        //Debug.Log("JUMP STAT 2: " + jumpStatus + " // " + trajectoryLine.enabled + " / " + targetDecal.activeSelf);
    }
}
