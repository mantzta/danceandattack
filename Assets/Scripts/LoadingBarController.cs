using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class LoadingBarController : MonoBehaviourPunCallbacks
{
    
    public Slider loadingBar; // Reference to the UI Image component representing the loading bar

    public string LoadBarPosition;

    private Saber redSaber; // Reference to the Saber script
    private Saber blueSaber;

    private float redFill = 0;
    private float blueFill = 0;

    private bool sabersInitialized = false;
    private bool isOpponentBar = false;

    private Photon.Realtime.Player opponent;
    private PowerBlockSpawner powerBlockSpawner;

    private int localSpawnPowerBlocks = 0;
    private int opponentSpawnPowerBlocks = 0;

    private float loadingFactor = 1.0f;

    private void Start()
    {
        redFill = 0;
        blueFill = 0;
        loadingBar.value = 0;
        powerBlockSpawner = FindObjectOfType<PowerBlockSpawner>();
        loadingFactor = 1.0f;
        localSpawnPowerBlocks = 0;
        opponentSpawnPowerBlocks = 0;
        sabersInitialized = false;
        isOpponentBar = false;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadBar", 0 } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "SpawnPowerBlocks", 0 } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PowerBlockData", null } });
    }

    public void InitializeSabers(Saber redSaber, Saber blueSaber)
    {
        if (!isOpponentBar)
        {
            this.redSaber = redSaber;
            this.blueSaber = blueSaber;
            sabersInitialized = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadBar", 0 } });
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "SpawnPowerBlocks", 0 } });
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PowerBlockData", null } });
            Debug.Log("INITIALIZED SABERS");
        }
    }

    public void InitializeOpponentBar()
    {
        // Choose the opponents props, left player is always the first player in the list.
        if (PhotonNetwork.PlayerList.Length > 1 && !sabersInitialized)
        {
            var isPlayerLeft = CheckPlayer.IsPlayerLeft();
            opponent = PhotonNetwork.PlayerList[isPlayerLeft ? 1 : 0];
            Debug.Log("INITIALIZED OPPONENT BAR");
            isOpponentBar = true;
        }

    }

    public void ChangeLoadingFactor(float factor)
    {
        loadingFactor = factor;
    }

    private string SerializePowerData(PowerBlockData data)
    {
        return $"{data.PowerId}_{data.AttackId}_{data.PowerPosition.x}_{data.PowerPosition.y}_{data.PowerPosition.z}_{data.AttackPosition.x}_{data.AttackPosition.y}_{data.AttackPosition.z}";
    }

    private PowerBlockData DeserializePowerData(string dataString)
    {
        string[] data = dataString.Split('_');
        int powerId = int.Parse(data[0]);
        int attackId = int.Parse(data[1]);
        Vector3 powerPosition = new Vector3(float.Parse(data[2]), float.Parse(data[3]), float.Parse(data[4]));
        Vector3 attackPosition = new Vector3(float.Parse(data[5]), float.Parse(data[6]), float.Parse(data[7]));

        return new PowerBlockData(powerPosition, attackPosition, powerId, attackId);
    }

    private void FixedUpdate()
    {
        if (sabersInitialized && !(PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Leaving))
        {
            if (redSaber != null && blueSaber != null)
            {
                // Assuming that Saber script has a public property to get the acceleration
                float redAcceleration = redSaber.GetAcceleration(); // Implement this method in Saber

                // Calculate fill amount based on acceleration (you might need to adjust these values)
                redFill += redAcceleration;

                // Assuming that Saber script has a public property to get the acceleration
                float blueAcceleration = blueSaber.GetAcceleration(); // Implement this method in Saber

                // Calculate fill amount based on acceleration (you might need to adjust these values)
                blueFill += blueAcceleration;

                //var fullFill = Mathf.Clamp01((redAcceleration + blueAcceleration) / 1000000);
                var fullFill = Mathf.Clamp01((redAcceleration + blueAcceleration) / (1000000 / loadingFactor));

                if (loadingBar.value >= 1)
                {
                    loadingBar.value = 0;
                    var isLeftPlayer = CheckPlayer.IsPlayerLeft();
                    PowerBlockData data = powerBlockSpawner.SpawnPowerBlocks(isLeftPlayer);

                    if (data != null) {
                        string dataString = SerializePowerData(data);
                        localSpawnPowerBlocks++;
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "SpawnPowerBlocks", localSpawnPowerBlocks } });
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "PowerBlockData", dataString } });
                    }
                    
                }

                loadingBar.value += fullFill;
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "LoadBar", loadingBar.value } });
            }

        }
        else if (isOpponentBar)
        {
            if (PhotonNetwork.PlayerList.Length > 1)
            {
                ExitGames.Client.Photon.Hashtable props = opponent.CustomProperties;
                if (props.ContainsKey("LoadBar") && props["LoadBar"] is float)
                {
                    // Fill the other players bar
                    float loadBar = (float)props["LoadBar"];
                    loadingBar.value = loadBar;

                    // Spawn the other players power and attack blocks too
                    if (props.ContainsKey("SpawnPowerBlocks") && props["SpawnPowerBlocks"] is int && (int) props["SpawnPowerBlocks"] > opponentSpawnPowerBlocks && props.ContainsKey("PowerBlockData") && props["PowerBlockData"] is string) {
                        opponentSpawnPowerBlocks = (int)props["SpawnPowerBlocks"];
                        PowerBlockData data = DeserializePowerData((string)props["PowerBlockData"]);

                        bool isPlayerLeft = CheckPlayer.IsPlayerLeft();
                        
                        powerBlockSpawner.SpawnOpponentBlocks(!isPlayerLeft, data);
                    }
                }
            }
        }
    }

}
