using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicActivation : MonoBehaviour
{
    [SerializeField] string themeMusicName = "";
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instance.PlayMusic(themeMusicName);
    }

}
