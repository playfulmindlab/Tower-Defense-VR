using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LSL4Unity.Samples.SimpleInlet;

public class JumpedTowerUI : MonoBehaviour
{
    GameControlManager gm;
    SimpleInletBalanceBoard balanceBoardInput;
    [SerializeField] Image gravityCenter;
    [SerializeField] Image reticle;

    public Transform ReticleTransform { get { return reticle.transform; } }

    //[SerializeField]

    // Start is called before the first frame update
    void Start()
    {
        gm = GameControlManager.instance;
        balanceBoardInput = gameObject.GetComponentInParent<SimpleInletBalanceBoard>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.jumpType == JumpedType.Normal)
        {
            gravityCenter.transform.localPosition = Vector3.Lerp(gravityCenter.transform.localPosition, balanceBoardInput.CoordValues / 2f, Time.deltaTime);
        }

        else
        {
            ReticleRaycast(Camera.main);
            //reticle.transform.position = Vector3.Lerp(reticle.transform.position, balanceBoardInput.CoordValues * 5f, Time.deltaTime);
        }
        //gravityCenter.transform.localPosition = balanceBoardInput.rotationValues / 2f;
    }

    public void ReticleRaycast(Camera camera)
    {
        // move the reticle to the screen point
        reticle.transform.localPosition = balanceBoardInput.CoordValues * 3;
        Debug.DrawRay(reticle.transform.position, reticle.transform.forward, Color.green);
    }
}
