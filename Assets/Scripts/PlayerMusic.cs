using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMusic : MonoBehaviour
{
    AudioSource audioSource;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayLevelMusic()
    {
        audioSource.clip = GameManager.Instance.GetLevelMusic();
        audioSource.Play();
    }

    public void LowerMusicVolume()
    {
        audioSource.volume = 0.2f;
    }

    public void DrivingMusicVolume()
    {
        audioSource.volume = 0.7f;
    }
}