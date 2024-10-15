using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class TunnellingVignette : MonoBehaviour
{
    Volume tvVolume;
    VolumeProfile vp;
    Vignette tvVignette;
    ClampedFloatParameter vIntensitry;

    [SerializeField] float transitionTime = 10;
    // Start is called before the first frame update
    void Start()
    {

        tvVolume = GetComponent<Volume>();
        vp = tvVolume.profile;
        tvVolume.profile.TryGet(out tvVignette);
        vIntensitry = tvVignette.intensity;
        Debug.Log("VOLUME: " + tvVolume);
        Debug.Log("VIGNETTE: " + tvVignette);
        //TunnelGOLocation();
    }

    float timer;

    private IEnumerator UpdateVignette()
    {
        timer = 0;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            float lerpValue = transitionTime / transitionTime;
            tvVignette.intensity.value = Mathf.Lerp(0f, 1f, lerpValue);
            yield return null;
        }
    }

    public void TunnelGOLocation()//Transform newTransform)
    {
        tvVignette.intensity.value = 0;
    }
}
