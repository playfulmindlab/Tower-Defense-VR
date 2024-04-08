using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherMenuButtons : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMenu()
    {
        GameManager.instance.ChangeScene("MainMenuVR");
    }
}
