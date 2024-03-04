using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class BeatSaberBlockSpawner : MonoBehaviourPunCallbacks
{
    public GameObject redBlockPrefab;
    public GameObject blueBlockPrefab;
    public GameObject bombBlockPrefab;
    public Transform spawnPoint;

    public float horizontalSpacing = 2.0f;
    public float verticalSpacing = 2.0f;

    public string beatMapFilePath;

    public float bpm = 105;

    public float noteJumpMovementSpeed = 10f;
    public float noteJumpStartBeatOffset = 1f;

    private Vector3 leftPlayerSpawnPosition = new Vector3(-2.0f, 1.5f, 15f); // Adjust these values as needed
    private Vector3 rightPlayerSpawnPosition = new Vector3(2.3f, 1.5f, 15f); // Adjust these values as needed


    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        string path = Path.Combine(Application.streamingAssetsPath, beatMapFilePath);

        Scene currentScene = SceneManager.GetActiveScene();

        if(currentScene.name != "SoloGameScene")
        {
            // Wait until both players pushed ready button
            yield return new WaitUntil(CheckIfBothPlayersAreReady);

            Debug.Log("ALL PLAYERS ACCEPTED");
        }

        yield return StartCoroutine(ReadFileFromStreamingAssets(path, (jsonString) =>
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                BeatSaberMapData mapData = JsonUtility.FromJson<BeatSaberMapData>(jsonString);

                StartCoroutine(SpawnBlocks(mapData));
            }
        }));
    }

    private IEnumerator ReadFileFromStreamingAssets(string filePath, System.Action<string> onComplete)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            Debug.LogError("File path is empty or null.");
            onComplete?.Invoke(null);
            yield break;
        }

        using (UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error reading file: " + www.error);
                onComplete?.Invoke(null);
            }
            else
            {
                onComplete?.Invoke(www.downloadHandler.text);
            }
        }
    }

    private IEnumerator SpawnBlocks(BeatSaberMapData mapData)
    {
        float startTime = Time.time;
        int id = 0;

        foreach (BeatSaberBlockData blockData in mapData._notes)
        {
            float targetTime = (blockData._time / bpm) * 60;


            float timeToWait = targetTime - (Time.time - startTime)+ noteJumpStartBeatOffset;

            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            SpawnBlock(blockData, id);
            id++;
        }
    }

    private bool CheckIfBothPlayersAreReady()
    {
        return FindObjectOfType<StartGameForBoth>().CheckIfAllPlayersReady();
    }

    private void SpawnBlock(BeatSaberBlockData blockData, int id)
    {
        Debug.Log("SPAWN " + blockData._time + "-" + blockData._type + " " +  spawnPoint.position + " " +  spawnPoint.rotation);

        Vector3 leftSpawnPosition = leftPlayerSpawnPosition;
        Vector3 rightSpawnPosition = rightPlayerSpawnPosition;

        leftSpawnPosition.x += blockData._lineIndex * horizontalSpacing;
        leftSpawnPosition.y += blockData._lineLayer * verticalSpacing;

        rightSpawnPosition.x += blockData._lineIndex * horizontalSpacing;
        rightSpawnPosition.y += blockData._lineLayer * verticalSpacing;

        Quaternion spawnRotation = spawnPoint.rotation;

        // Serialize the BeatSaberBlockData
        string serializedData = blockData.Serialize();

        SpawnBlockRPC(serializedData, leftSpawnPosition, rightSpawnPosition, spawnRotation, id);
    }

    private void SpawnBlockRPC(string serializedData, Vector3 leftPosition, Vector3 rightPosition, Quaternion rotation, int id)
    {
        // Deserialize the serialized data
        BeatSaberBlockData blockData = BeatSaberBlockData.Deserialize(serializedData);
        if (blockData == null)
        {
            Debug.LogError("Failed to deserialize block data.");
            return;
        }

        GameObject blockPrefab = GetBlockPrefab(blockData._type);

        GameObject leftBlockInstance = Instantiate(blockPrefab, leftPosition, rotation);
        GameObject rightBlockInstance = Instantiate(blockPrefab, rightPosition, rotation);

        InitializeBlockController(leftBlockInstance, blockData, true, id);
        InitializeBlockController(rightBlockInstance, blockData, false, id);
    }

    private void InitializeBlockController(GameObject blockInstance, BeatSaberBlockData blockData, bool isLeftPlayer, int id)
    {
        BlockController blockController = blockInstance.GetComponent<BlockController>();
        Vector3 moveDirection = new Vector3(0, 0, -1);

        //if (playerPosition == "left")
        //{

        //    moveDirection = new Vector3(-1, 0, -1); // turns cubes 45 degrees clock wise
        //}

        //if (playerPosition == "right")
        //{
        //    moveDirection = new Vector3(1, 0, -1); // turns cubes 45 degrees anti clock wise
        //}

        blockController.Initialize(blockData, moveDirection, isLeftPlayer, id);
    }
    private GameObject? GetBlockPrefab(int blockType)
    {
        switch (blockType)
        {
            case (int)BeatSaberBlockType.Red:
                return redBlockPrefab;
            case (int)BeatSaberBlockType.Blue:
                return blueBlockPrefab;
            case (int)BeatSaberBlockType.Bomb:
                return bombBlockPrefab;
            default:
                Debug.LogError("Unknown block type: " + blockType);
                return null;
        }
    }

    [System.Serializable]
    public class BeatSaberMapData
    {
        public BeatSaberBlockData[] _notes;
    }

    [System.Serializable]
    public class BeatSaberBlockData
    {
        public float _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _type;
        public int _cutDirection;

        public string Serialize()
        {
            // Serialize the data as a string, you can use a specific format
            return $"{_time},{_lineIndex},{_lineLayer},{_type},{_cutDirection}";
        }

        public static BeatSaberBlockData Deserialize(string serializedData)
        {
            // Deserialize the string back into BeatSaberBlockData
            string[] parts = serializedData.Split(',');
            if (parts.Length == 5)
            {
                return new BeatSaberBlockData
                {
                    _time = float.Parse(parts[0]),
                    _lineIndex = int.Parse(parts[1]),
                    _lineLayer = int.Parse(parts[2]),
                    _type = int.Parse(parts[3]),
                    _cutDirection = int.Parse(parts[4])
                };
            }
            else
            {
                Debug.LogError("Invalid serialized data format.");
                return null;
            }
        }
    }


    public enum BeatSaberBlockType
    {
        Red, Blue, Bomb, Power, Attack
    };
}

