using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private bool isRunning;
    public float maxTime = 15.0f;
    private float remainingTime;

    public TextMeshProUGUI timerText;

    public bool IsRunning { get => isRunning; set => isRunning = value; }
    public float RemainingTime { get => remainingTime; set => remainingTime = value; }

    private void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning){
            remainingTime -= Time.deltaTime;

            timerText.text = Extentions.Reverse(((int)remainingTime).ToString());
            if (remainingTime < 0)
                RanOut();
        }
    }

    public void StartTimer()
    {
        RemainingTime = maxTime;
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    void RanOut()
    {
        Stop();
        GameManager.Instance.TimerRanOut();
    }
}
