using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] GameObject sfxPrefab;
    [SerializeField] Sound[] musicArray, sfxArray;

    [SerializeField] AudioSource musicSource;//, sfxSource;

    
    Sound musicSound, sfxSound;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this.gameObject);
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

    public void PlaySFX(string sfxName, Vector3 sfxPosition)
    {
        sfxSound = Array.Find(sfxArray, x => x.name == sfxName);

        if (sfxSound != null)
        {
            GameObject tempSFXObject = Instantiate(sfxPrefab, sfxPosition, Quaternion.identity);
            AudioSource newSFX = tempSFXObject.GetComponent<AudioSource>();
            newSFX.PlayOneShot(sfxSound.clip, sfxSound.volume);
            Destroy(tempSFXObject, sfxSound.clip.length);
        }
        else Debug.LogError("Sound '" + sfxName + "' not present in AudioManager: SFXArray!");
    }

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
    }

    /*public void PlaySFX(string sfxName)
    {
        sfxSound = Array.Find(sfxArray, x => x.name == sfxName);

        if (sfxSound != null)
        {
            //sfxSource.PlayOneShot(sfxSound.clip);
        }
        else Debug.LogError("Sound '" + sfxName + "' not present in AudioManager: SFXArray!");
    }*/
}
