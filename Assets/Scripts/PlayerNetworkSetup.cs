using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetworkSetup : MonoBehaviourPunCallbacks
{
    public GameObject LocalXRRigGameObject;

    public GameObject AvatarHeadGameObject;
    public GameObject AvatarBodyGameObject;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            // The player is local
            LocalXRRigGameObject.SetActive(true);
            SetLayerRecursively(AvatarHeadGameObject, 3);
            SetLayerRecursively(AvatarBodyGameObject, 3);

        } else
        {
            // The player is remote
            LocalXRRigGameObject.SetActive(false);
            SetLayerRecursively(AvatarHeadGameObject, 0);
            SetLayerRecursively(AvatarBodyGameObject, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Gameobjects have children so you will need to set each childs layer as well to make sure it is visible or hidden
    void SetLayerRecursively(GameObject go, int layerNumber)
    {
        if (go == null) return;
        foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = layerNumber;
        }
    }
}
