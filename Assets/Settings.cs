using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    GameManager gm;

    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider xSensitivity;
    [SerializeField] Slider ySensitivity;
    [SerializeField] Toggle yInvert;

    [SerializeField] TMPro.TMP_Text musicText;
    [SerializeField] TMPro.TMP_Text sfxText;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameManager.instance;

        ResetToDefaultSettings();
        gm.UpdateSettings();
    }

    public void UpdateMusicVolume(float newValue)
    {
        gm.musicVolume = newValue;
        musicText.text = newValue.ToString("F3");
        gm.UpdateSettings();
    }

    public void UpdateSFXVolume(float newValue)
    {
        gm.sfxVolume = newValue;
        sfxText.text = newValue.ToString("F3");
        gm.UpdateSettings();
    }

    public void UpdateXSensitivity(float newValue)
    {
        gm.balanceBoardXSensitivity = newValue;
        gm.UpdateSettings();
    }

    public void UpdateYSensitivity(float newValue)
    {
        gm.balanceBoardYSensitivity = newValue;
        gm.UpdateSettings();
    }

    public void UpdateYInverse(bool newToggle)
    {
        if (newToggle == true) gm.balanceBoardYInversion = -1f;
        else gm.balanceBoardYInversion = 1f;

        gm.UpdateSettings();
    }

    public void ResetToDefaultSettings()
    {
        musicVolumeSlider.value = 1.0f;
        sfxVolumeSlider.value = 1.0f;
        xSensitivity.value = 1.0f;
        ySensitivity.value = 1.0f;
        yInvert.isOn = false;

        musicText.text = musicVolumeSlider.value.ToString("F3");
        sfxText.text = sfxVolumeSlider.value.ToString("F3");

        UpdateMusicVolume(musicVolumeSlider.value);
        UpdateSFXVolume(sfxVolumeSlider.value);
        UpdateXSensitivity(xSensitivity.value);
        UpdateYSensitivity(ySensitivity.value);
        UpdateYInverse(yInvert.isOn);
    }


}
