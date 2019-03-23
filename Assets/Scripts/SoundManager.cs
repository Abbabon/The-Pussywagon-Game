using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance { get { return _instance; } }
    private static readonly object padlock = new object();

    [Space(10)]
    private AudioSource currentAudioSource;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("AWAKE");
        lock (padlock)
        {
            if (_instance != null && _instance != this)
            {
                Debug.Log("DESTROY");
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
                LoadSoundEffects();
            }
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayLevelMusic(){
        if (currentAudioSource != null){
            Debug.Log(String.Format("GettingLevelMusic for {0}", SceneManager.GetActiveScene().buildIndex));
            switch (SceneManager.GetActiveScene().buildIndex)
            {
                case 0:
                    currentAudioSource.clip = Resources.Load<AudioClip>("Music/Yalda");
                    StartBackgroundMusic();
                    return;
                    //TODO: return this when finding some better soundtrack
                case 1:
                    currentAudioSource.clip = Resources.Load<AudioClip>("Music/Beitar");
                    StartBackgroundMusic();
                    return;
                default:
                    currentAudioSource.clip = null;
                    return;
            }
        }
        Debug.Log("Current Audio Source is not found!");
    }

    public void RegisterAudioSource(AudioSource audioSource)
    {
        currentAudioSource = audioSource;
    }

    public void StartBackgroundMusic()
    {
        currentAudioSource.Play();
    }

    public void StopBackgroundMusic()
    {
        currentAudioSource.Play();
    }

    public void LowerMusicVolume()
    {
        currentAudioSource.volume = 0.2f;
    }

    public void DrivingMusicVolume()
    {
        currentAudioSource.volume = 0.7f;
    }

    #region SoundEffects

    public enum SoundEffect
    {
        carStart,
        cash,
        crashHole,
        crashPolice,
        crashLamed,
        flash,
        siren,
        stop
    }

    private Dictionary<SoundEffect, AudioClip> soundEffects;
    private void LoadSoundEffects()
    {
        soundEffects = new Dictionary<SoundEffect, AudioClip>();
        foreach (SoundEffect soundEffect in (SoundEffect[])Enum.GetValues(typeof(SoundEffect))){
            soundEffects.Add(soundEffect, Resources.Load<AudioClip>(String.Format("Effects/{0}", soundEffect)));
        }
    }

    public void PlaySoundEffect(SoundEffect soundEffect)
    {
        currentAudioSource.PlayOneShot(soundEffects[soundEffect]);
    }

    #endregion

}
