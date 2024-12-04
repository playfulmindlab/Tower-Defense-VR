using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FMODEventManager : MonoBehaviour
{
    public static FMODEventManager instance;

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy (this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
