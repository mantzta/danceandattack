using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayMusicWithDelay : MonoBehaviour
{

    public float delay = 1;
    UnityEngine.SceneManagement.Scene currentScene;

    // Start is called before the first frame update
    void Awake()
    {
        currentScene = SceneManager.GetActiveScene();

        if (currentScene.name != "SoloGameScene")
        {
            StartCoroutine(WaitForPlayers());
        } else
        {
            Invoke("PlayMusic", delay);
        }
    }

    private IEnumerator WaitForPlayers()
    {
        Debug.Log("Waiting for players to start music....");
        // Wait until both players pushed ready button
        yield return new WaitUntil(CheckIfBothPlayersAreReady);
        Debug.Log("Both players are ready to listen to music!");
        StartCoroutine(InvokeMusic());
        
    }

    private bool CheckIfBothPlayersAreReady()
    {
        return FindObjectOfType<StartGameForBoth>().CheckIfAllPlayersReady();
    }

    private IEnumerator InvokeMusic()
    {
        yield return null;
        Invoke("PlayMusic", delay);
    }

    private void PlayMusic()
    {
        Debug.Log("MUSIC IS STARTING TO PLAY");
        
        GetComponent<AudioSource>().Play();
    }
}
