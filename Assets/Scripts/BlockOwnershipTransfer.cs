using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class BlockOwnershipTransfer : MonoBehaviourPunCallbacks
{
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Transfer ownership of the object to the specified player
    public void TransferOwnershipToPlayer(Player newOwner)
    {
        // Check if the object has a PhotonView attached
        if (photonView != null)
        {
            // Transfer ownership to the new owner
            photonView.TransferOwnership(newOwner);
        }
    }
}
