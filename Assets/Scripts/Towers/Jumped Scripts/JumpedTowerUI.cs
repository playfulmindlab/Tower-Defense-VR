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
        if (gm.IsJumped)
        {
            if (gm.jumpType == JumpedType.Normal)
            {
                if (balanceBoardInput.CoordValues.x != float.NaN && balanceBoardInput.CoordValues.y != float.NaN)
                    gravityCenter.transform.localPosition = Vector3.Lerp(gravityCenter.transform.localPosition, balanceBoardInput.CoordValues / 2f, Time.deltaTime);
            }

            else
            {
                ReticleRaycast(Camera.main);
                //reticle.transform.position = Vector3.Lerp(reticle.transform.position, balanceBoardInput.CoordValues * 5f, Time.deltaTime);
            }
        }
        
    }

    Vector3 bbReticleMod = new Vector3(1f, -1f, 1f);
    public void ReticleRaycast(Camera camera)
    {
        // move the reticle to the screen point

        reticle.transform.localPosition = Vector3.Lerp(reticle.transform.localPosition, balanceBoardInput.CoordValues * 3f * bbReticleMod, Time.deltaTime * gm.cameraDamping);//balanceBoardInput.CoordValues * 3;
        reticle.transform.rotation = Quaternion.LookRotation((Camera.main.transform.position - reticle.transform.position).normalized);

        //Debug.DrawRay(reticle.transform.position, reticle.transform.forward, Color.red);
        //Debug.DrawRay(reticle.transform.position, -reticle.transform.forward, Color.green);
    }

    public void ResetReticle()
    {
        reticle.transform.localPosition = Vector3.zero;
        reticle.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
