using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandler : MonoBehaviour
{

    public void StartGame()
    {
        GameManager.Instance.StartGame();
    }

    public void ToMainMenu()
    {
        GameManager.Instance.ToMainMenu();
    }

    public void RestartLevel()
    {
        GameManager.Instance.RestartLevel();
    }

    public void FinishedTutorial()
    {
        GameManager.Instance.FinishedTutorial();
    }

    public void NextLevel()
    {
        GameManager.Instance.NextLevel();
    }

    public void EscapeDialogue()
    {
        SoundManager.Instance.PlayRandomDialogue(SoundManager.DialogueCategories.BatutaBye);
        GameManager.Instance.CloseDialogue();
    }

    public void PauseGame()
    {
        GameManager.Instance.TogglePause();
    }

    public void PauseMovement()
    {
        GameManager.Instance.ToggleMovementPause();
    }

    public void AddCops()
    {
        GameManager.Instance.CallThePopo();
    }

}
