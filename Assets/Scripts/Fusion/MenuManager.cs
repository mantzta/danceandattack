using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadSoloGame()
    {
        SceneManager.LoadScene("SoloGameScene");
    }

    public void LoadMultiplayerGame()
    {
        SceneManager.LoadScene("MultiGameSceneFusion");
    }
}
