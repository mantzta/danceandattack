using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LoginManager : MonoBehaviourPunCallbacks
{
    public void LoadSoloGame()
    {
        SceneManager.LoadScene("SoloGameScene");
    }

    public void ConnectAnonymously()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Photon Callback Methods
    public override void OnConnected()
    {
        Debug.Log("OnConnected is called! The server is available!");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server!");
        PhotonNetwork.LoadLevel("MultiStartScene");
    }
    #endregion
}
