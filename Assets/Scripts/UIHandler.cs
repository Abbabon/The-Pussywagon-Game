using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    public void StartGame()
    {
        //TODO: play the relevant sounds
        GameManager.Instance.StartGame();
    }


    public void RestartLevel()
    {
        //TODO: play the relevant sounds
        GameManager.Instance.RestartLevel();
    }

    public void EscapeDialogue()
    {
        //TODO: play the relevant sounds
        GameManager.Instance.CloseDialogue();
    }

    public void PauseGame()
    {
        GameManager.Instance.TogglePause();
    }

    public void AddCops()
    {
        GameManager.Instance.CallThePopo();
    }

}
