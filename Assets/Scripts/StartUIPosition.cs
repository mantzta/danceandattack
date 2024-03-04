using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartUIPosition : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            transform.position = new Vector3(-2, gameObject.transform.position.y, gameObject.transform.position.z);
        }
    }
}
