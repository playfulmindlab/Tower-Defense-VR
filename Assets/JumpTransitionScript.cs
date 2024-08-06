using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTransitionScript : MonoBehaviour
{
    GameControlManager gcm;
    // Start is called before the first frame update
    void Start()
    {
        gcm = GameControlManager.instance;
    }

    public void ControlSwapEvent(string controlName)
    {
        gcm.SwapControls(controlName);
    }
}
