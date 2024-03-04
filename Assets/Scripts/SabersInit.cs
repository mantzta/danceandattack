using UnityEngine;
using Photon.Pun;
using System.Linq;

public class SabersInit : MonoBehaviourPunCallbacks
{
    private Saber redSaber;
    private Saber blueSaber;

    private string opponentBarTag;

    private ScoreManager scoreManager;

    private void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();

        if (photonView.IsMine)
        {
            // Check if this is the master client
            var isPlayerLeft = CheckPlayer.IsPlayerLeft();
            if (isPlayerLeft)
            {
                // Look for the LeftLoadingBar and initialize sabers
                opponentBarTag = "RightLoadingBar";
                FindAndInitializeSabers("LeftLoadingBar");
            }
            else
            {
                // Look for the RightLoadingBar and initialize sabers
                opponentBarTag = "LeftLoadingBar";
                FindAndInitializeSabers("RightLoadingBar");
            }

            InvokeRepeating("InitialiseOpponentBar", 0, 1);
        }
    }

    private void FindAndInitializeSabers(string loadingBarTag)
    {
        // Find the loading bar by its tag
        GameObject loadingBar = GameObject.FindGameObjectWithTag(loadingBarTag);

        if (loadingBar != null)
        {
                LoadingBarController loadingBarController = loadingBar.GetComponent<LoadingBarController>();
                //GameObject opponentBar = GameObject.FindGameObjectWithTag(opponentBarTag);

            if (loadingBarController != null)
            {
                 GameObject[] redSabers = FindSabers("RedSaber");

                 if (redSabers.Length > 0)
                 {
                    redSaber = redSabers[0].GetComponent<Saber>();
                    redSaber.whichSaber = CheckPlayer.IsPlayerLeft() ? "LEFT RED SABER" : "RIGHT RED SABER";
                 }

                 GameObject[] blueSabers = FindSabers("BlueSaber");

                 if (blueSabers.Length > 0)
                 {
                    blueSaber = blueSabers[0].GetComponent<Saber>();
                    blueSaber.whichSaber = CheckPlayer.IsPlayerLeft() ? "LEFT BLUE SABER" : "RIGHT BLUE SABER";
                 }

                if (redSaber != null && blueSaber != null)
                {
                    loadingBarController.InitializeSabers(redSaber, blueSaber);
                    //opponentBar.gameObject.SetActive(false); // hide opponents bar
                    scoreManager.redSaber = redSaber;
                    scoreManager.blueSaber = blueSaber;
                }
                else
                {
                    Debug.LogError("No Sabers found for this player");
                }
            }
        } 
        else
        {
            Debug.LogError("Loading bar not found with tag: " + loadingBarTag);
        }
    }

    private void InitialiseOpponentBar()
    {
        if (PhotonNetwork.PlayerList.Length > 1)
        {
            GameObject opponentBar = GameObject.FindGameObjectWithTag(opponentBarTag);

            if (opponentBar != null)
            {
                LoadingBarController opponentLoadingBarController = opponentBar.GetComponent<LoadingBarController>();

                opponentLoadingBarController.InitializeOpponentBar();

                CancelInvoke("InitialiseOpponentBar");
            }
            else
            {
                Debug.LogError("Opponent loading bar not found with tag: " + opponentBarTag);
            }
        }
    }

    private GameObject[] FindSabers(string saberTag)
    {
        // Find all GameObjects with the "RedSaber" tag that are children of the parentGameObject
        return gameObject.GetComponentsInChildren<Transform>()
            .Where(child => child.CompareTag(saberTag))
            .Select(child => child.gameObject)
            .ToArray();
    }

    //public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    //{
    //    var playerPos = PhotonNetwork.MasterClient == targetPlayer ? "LEFT" : "RIGHT";
    //    Debug.Log("HI I AM PLAYER " + playerPos + " with changed props: " + changedProps);
    //}
}
