using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    public void StartGame()
    {
        GameManager.Instance.StartGame();
    }


    public void RestartLevel()
    {
        GameManager.Instance.RestartLevel();
    }

    public void EscapeDialogue()
    {
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
