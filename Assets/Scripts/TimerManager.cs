using System.Collections;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float countdownDuration = 60.0f; // Set the countdown duration in seconds

    private float currentTime;

    private void Start()
    {
        currentTime = countdownDuration;
        StartCoroutine(WaitForPlayers());
    }

    private IEnumerator WaitForPlayers()
    {
        // Wait until both players pushed ready button
        yield return new WaitUntil(CheckIfBothPlayersAreReady);
        InvokeRepeating("UpdateTimer", 0.0f, 1.0f); // Update the timer every 1 second
    }

    private bool CheckIfBothPlayersAreReady()
    {
        return FindObjectOfType<StartGameForBoth>().CheckIfAllPlayersReady();
    }

    private void UpdateTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= 1.0f;

            // Calculate minutes and seconds
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            // Update the timer text
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            // Countdown is complete, handle the end of the countdown
            // You can add your logic for what happens when the countdown reaches 0 here
            timerText.text = "00:00";
            CancelInvoke("UpdateTimer"); // Stop updating the timer
        }
    }
}

