using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Music
{
    public string name;
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
}

[System.Serializable]
public class SoundClip
{
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
}

[System.Serializable]
public class Sounds
{
    public string name;
    public SoundClip[] clips;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] GameObject sfxPrefab;
    [SerializeField] Music[] musicArray;
    [SerializeField] Sounds[] sfxArray;

    [SerializeField] AudioSource musicSource;//, sfxSource;
    float sfxVolumeMod = 1f;

    Music musicSound;
    Sounds sound;
    SoundClip sfxSound;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);

        musicSource = GetComponent<AudioSource>();
        DontDestroyOnLoad(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        //PlayMusic("MainTheme");
    }

    public void PlayMusic(string musicName)
    {
        musicSound = Array.Find(musicArray, x => x.name == musicName);

        if (musicSound != null)
        {
            musicSource.clip = musicSound.clip;
            musicSource.volume = musicSound.volume;
            musicSource.Play();
        }
        else Debug.LogError("Sound '" + musicName + "' not present in AudioManager: MusicArray!");
    }

    public void UpdateMusicVolume(float newVolume)
    {
        musicSource.volume = newVolume;
    }

    public void UpdateSFXVolume(float newVolume)
    {
        sfxVolumeMod = newVolume;
    }

    public void PlaySFXArray(string sfxName, Vector3 sfxPosition)
    {
        sound = Array.Find(sfxArray, x => x.name == sfxName);

        if (sound != null)
        {
            if (sound.clips.Length > 0) {
                int clipNum = UnityEngine.Random.Range(0, sound.clips.Length);
                SoundClip sfxClip = sound.clips[clipNum];
            
                GameObject tempSFXObject = Instantiate(sfxPrefab, sfxPosition, Quaternion.identity);
                AudioSource newSFX = tempSFXObject.GetComponent<AudioSource>();
                newSFX.PlayOneShot(sfxClip.clip, sfxClip.volume * sfxVolumeMod);
                Destroy(tempSFXObject, sfxClip.clip.length); 
            }
            else Debug.LogError("Sound '" + sfxName + "' has an empty array!");
        }
        else Debug.LogError("Sound '" + sfxName + "' not present in AudioManager: SFXArray!");
    }

    /*
    public void PlaySFXPitched(string sfxName, Vector3 sfxPosition)//, float volume = 1f)
    {
        sfxSound = Array.Find(sfxArray, x => x.name == sfxName);

        if (sfxSound != null)
        {
            GameObject tempSFXObject = Instantiate(sfxPrefab, sfxPosition, Quaternion.identity);
            AudioSource newSFX = tempSFXObject.GetComponent<AudioSource>();
            newSFX.pitch = UnityEngine.Random.Range(0.5f, 2f);
            newSFX.PlayOneShot(sfxSound.clip, sfxSound.volume);
            Destroy(tempSFXObject, sfxSound.clip.length);
        }
        else Debug.LogError("Sound '" + sfxName + "' not present in AudioManager: SFXArray!");
    }

    public void PlaySFXRandom(string sfxName, Vector3 sfxPosition, int maxRandom)//, float volume = 1f)
    {
        int sfxNum = UnityEngine.Random.Range(1, maxRandom + 1);
        string newSFXName = sfxName + sfxNum;
        PlaySFX(newSFXName, sfxPosition);//, volume);
    }*/
}
