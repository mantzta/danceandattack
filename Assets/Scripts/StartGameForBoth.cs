using Photon.Pun;
using UnityEngine;

public class StartGameForBoth : MonoBehaviourPunCallbacks
{
    public bool IsPlayerReady = false;
    public GameObject UICanvasGameObject;
    private bool StartSoloGame = false;

    public void SetPlayerToReady()
    {
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            SetSoloGame();
        }

        // Set the local player as ready
        IsPlayerReady = true;
        UICanvasGameObject.SetActive(false);

        ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable
            {
                { "IsPlayerReady", IsPlayerReady }
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
    }

    public void SetSoloGame()
    {
        StartSoloGame = true;
        UICanvasGameObject.SetActive(false);
    }

    public bool CheckIfAllPlayersReady()
    {
        if(StartSoloGame)
        {
            //StartSoloGame = false;
            return true;
        }

        if (PhotonNetwork.PlayerList.Length < 2)
        {
            return false;
        }

        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        var countTrue = 0;

        foreach (Photon.Realtime.Player player in players)
        {
            if (player.CustomProperties.TryGetValue("IsPlayerReady", out object isReady) && (bool) isReady)
            {
                countTrue++;
            }
        }

        if (countTrue == players.Length)
        {
            return true;
        } else
        {
            return false;
        }
    }

}
