using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class HardwareRig : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Rig Components")]
    public Transform _characterTransform;
    public Transform _headTransform;
    public Transform _bodyTransform;
    public Transform _leftHandTransform;
    public Transform _rightHandTransform;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManagerFusion.Instance.SessionRunner.AddCallbacks(this);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        XRRigInputData inputData = new XRRigInputData();

        inputData.HeadsetPosition = _headTransform.position;
        inputData.HeadsetRotation = _headTransform.rotation;

        inputData.BodyPosition = _bodyTransform.position;
        inputData.BodyRotation = _bodyTransform.rotation;

        inputData.CharacterPosition = _characterTransform.position;
        inputData.CharacterRotation = _characterTransform.rotation;

        inputData.LefthandPosition = _leftHandTransform.position;
        inputData.LefthandRotation = _leftHandTransform.rotation;

        inputData.RighthandPosition = _rightHandTransform.position;
        inputData.RighthandRotation = _rightHandTransform.rotation;

        input.Set(inputData);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        
    }
}

public struct XRRigInputData : INetworkInput
{
    public Vector3 MainPlayerPosition;
    public Quaternion MainPlayerRotation;

    public Vector3 HeadsetPosition;
    public Quaternion HeadsetRotation;

    public Vector3 BodyPosition;
    public Quaternion BodyRotation;

    public Vector3 CharacterPosition;
    public Quaternion CharacterRotation;

    public Vector3 LefthandPosition;
    public Quaternion LefthandRotation;

    public Vector3 RighthandPosition;
    public Quaternion RighthandRotation;
}
