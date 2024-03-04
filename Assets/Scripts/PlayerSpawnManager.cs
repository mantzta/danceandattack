using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField]
    GameObject GenericVRPlayerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            var nickName = DetermineNickNameOfPlayer();

            if (nickName == "")
            {
                throw AllSpotsTakenException();
            }

            var xPosition = nickName == "LeftPlayer" ? -1 : 3;

            // Calculate the spawn position based on player count
            Vector3 position = new Vector3(xPosition, 0, 1.5f);
            GameObject playerInstance = PhotonNetwork.Instantiate(GenericVRPlayerPrefab.name, position, Quaternion.identity);

            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "NickName", nickName } });

            Debug.Log($"Spawned player {nickName} at position x={position.x}");
        }
    }

    private System.Exception AllSpotsTakenException()
    {
        throw new System.ApplicationException("All spots are taken already for spawning player with custom props" + PhotonNetwork.LocalPlayer.CustomProperties);
    }

    private string DetermineNickNameOfPlayer()
    {
        var players = PhotonNetwork.PlayerList;
        bool rightIsOccupied = false;
        bool leftIsOccupied = false;
        var nickName = "";

        foreach(var player in players)
        {
            if (player.CustomProperties.ContainsKey("NickName") && (string) player.CustomProperties["NickName"] == "LeftPlayer")
            {
                if (player == PhotonNetwork.LocalPlayer)
                {
                    Debug.Log("I want to spawn myself but I have already a NickName: LeftPlayer");
                    nickName = "LeftPlayer";
                }

                leftIsOccupied = true;
            }

            if (player.CustomProperties.ContainsKey("NickName") && (string) player.CustomProperties["NickName"] == "RightPlayer")
            {
                if (player == PhotonNetwork.LocalPlayer)
                {
                    Debug.Log("I want to spawn myself but I have already a NickName: RightPlayer");
                    nickName = "RightPlayer";
                }

                rightIsOccupied = true;
            }
        }

        if ((nickName == "LeftPlayer") || (!leftIsOccupied && !rightIsOccupied) || (!leftIsOccupied && rightIsOccupied))
        {
            Debug.Log("Both spots free: " + (!leftIsOccupied && !rightIsOccupied) + " only left free: " + (!leftIsOccupied && rightIsOccupied) + " nickName: " + nickName);
            nickName = "LeftPlayer";
        } else if ((nickName == "RightPlayer") || (leftIsOccupied && !rightIsOccupied))
        {
            Debug.Log("only right free: " + (leftIsOccupied && !rightIsOccupied) + " nickName: " + nickName);
            nickName = "RightPlayer";
        } else
        {
            Debug.LogError("Both positions seem to be full already");
            nickName = "";
        }

        return nickName;
    }
}

public static class CheckPlayer
{
    public static bool IsPlayerLeft()
    {
        var nickName = (string)PhotonNetwork.LocalPlayer.CustomProperties["NickName"];

        if (nickName != null && nickName != "")
        {
            return nickName == "LeftPlayer";
        } else
        {
            return PhotonNetwork.IsMasterClient;
        }
    }

    public static bool IsPlayerRight()
    {
        var nickName = (string)PhotonNetwork.LocalPlayer.CustomProperties["NickName"];

        if (nickName != null && nickName != "")
        {
            return nickName == "RightPlayer";
        }
        else
        {
            return !PhotonNetwork.IsMasterClient;
        }
    }
}
