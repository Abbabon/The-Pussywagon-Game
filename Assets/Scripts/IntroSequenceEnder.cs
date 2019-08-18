using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class IntroSequenceEnder : MonoBehaviour
{
    PlayableDirector sequence;
    public GameObject blackScreen;

    private void Awake()
    {
        sequence = GetComponent<PlayableDirector>();
    }

    private void Update()
    {
        if (sequence.state != PlayState.Playing)
        {
            blackScreen.SetActive(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}
