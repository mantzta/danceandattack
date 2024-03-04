using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiplayerVRConstants
{
    public const string MAP_TYPE_KEY = "map";
    public const string MAP_TYPE_VALUE_MULTI_A = "Multiplayer_Game_A";
    public const string MAP_TYPE_VALUE_MULTI_B = "Multiplayer_Game_B";
}

public class RoomManager : MonoBehaviourPunCallbacks
{
    private string mapType;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void GoBackToStart()
    {
        //PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
    }

    public override void OnLeftLobby()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LoadLevel("StartScene");
    }

    public void OnMultiButtonCLicked_A()
    {
        mapType = MultiplayerVRConstants.MAP_TYPE_VALUE_MULTI_A;
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { MultiplayerVRConstants.MAP_TYPE_KEY, mapType } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    public void OnMultiButtonCLicked_B()
    {
        mapType = MultiplayerVRConstants.MAP_TYPE_VALUE_MULTI_B;
        ExitGames.Client.Photon.Hashtable expectedCustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { MultiplayerVRConstants.MAP_TYPE_KEY, mapType } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    #region PUN Callback Methods
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined the Lobby");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        CreateAndJoinRoom();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to servers AGAIN");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("A room is created: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Local Player joind the room: " + PhotonNetwork.CurrentRoom.Name + " Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MultiplayerVRConstants.MAP_TYPE_KEY))
        {
            object mapType;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(MultiplayerVRConstants.MAP_TYPE_KEY, out mapType))
            {
                Debug.Log("Joined room with the map: " + (string)mapType);

                if ((string)mapType == MultiplayerVRConstants.MAP_TYPE_VALUE_MULTI_A)
                {
                    PhotonNetwork.LoadLevel("MultiGameSceneA");
                }
                else if ((string)mapType == MultiplayerVRConstants.MAP_TYPE_VALUE_MULTI_B)
                {
                    PhotonNetwork.LoadLevel("MultiGameSceneB");
                }
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A player joined the room. Player count: " + PhotonNetwork.CurrentRoom.PlayerCount);
    }
    #endregion

    #region Private Methods
    private void CreateAndJoinRoom()
    {
        string randomRoomName = "Room_" + mapType + "_" + Random.Range(0, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 20;

        string[] roomPropsInLobby = { MultiplayerVRConstants.MAP_TYPE_KEY };

        ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable() { { MultiplayerVRConstants.MAP_TYPE_KEY, mapType } };
        roomOptions.CustomRoomPropertiesForLobby = roomPropsInLobby;
        roomOptions.CustomRoomProperties = customRoomProperties;

        PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
    }
    #endregion
}
