using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkRig : NetworkBehaviour
{
    public bool isLocalNetworkRig => Object.HasStateAuthority;

    [Header("Rig Visuals")]
    [SerializeField]
    private GameObject _headVisuals;

    [Header("RigComponents")]
    [SerializeField]
    private NetworkTransform _characterTransform;

    [SerializeField]
    private NetworkTransform _headTransform;

    [SerializeField]
    private NetworkTransform _bodyTransform;

    [SerializeField]
    private NetworkTransform _leftHandTransform;

    [SerializeField]
    private NetworkTransform _rightHandTransform;

    HardwareRig _hardwareRig;

    public override void Spawned()
    {
        base.Spawned();

        if (isLocalNetworkRig)
        {
            _hardwareRig = FindObjectOfType<HardwareRig>();
            if (_hardwareRig == null)
            {
                Debug.Log("Cant find hardware rig in the scene");
            }
            _headVisuals.SetActive(false);
        } 
        else
        {
            Debug.Log("This is a client object");
        }
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput<XRRigInputData>(out var inputData))
        {
            _characterTransform.transform.SetPositionAndRotation(inputData.CharacterPosition, inputData.CharacterRotation);

            _headTransform.transform.SetPositionAndRotation(inputData.HeadsetPosition, inputData.HeadsetRotation);

            _bodyTransform.transform.SetPositionAndRotation(inputData.BodyPosition, inputData.BodyRotation);

            _leftHandTransform.transform.SetPositionAndRotation(inputData.LefthandPosition, inputData.LefthandRotation);

            _rightHandTransform.transform.SetPositionAndRotation(inputData.RighthandPosition, inputData.RighthandRotation);
        }
    }

    public override void Render()
    {
        base.Render();

        if(isLocalNetworkRig)
        {

            _characterTransform.transform.SetPositionAndRotation(_hardwareRig._characterTransform.position, _hardwareRig._characterTransform.rotation);

            _headTransform.InterpolationTarget.SetPositionAndRotation(_hardwareRig._headTransform.position, _hardwareRig._headTransform.rotation);

            _bodyTransform.transform.SetPositionAndRotation(_hardwareRig._bodyTransform.position, _hardwareRig._bodyTransform.rotation);

            _leftHandTransform.transform.SetPositionAndRotation(_hardwareRig._leftHandTransform.position, _hardwareRig._leftHandTransform.rotation);

            _rightHandTransform.transform.SetPositionAndRotation(_hardwareRig._rightHandTransform.position, _hardwareRig._rightHandTransform.rotation);
        }
    }
}
