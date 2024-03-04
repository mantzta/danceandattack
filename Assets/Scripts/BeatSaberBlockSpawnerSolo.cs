using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class BeatSaberBlockSpawnerSolo : MonoBehaviour
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

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        string path = Path.Combine(Application.streamingAssetsPath, beatMapFilePath);
        yield return StartCoroutine(ReadFileFromStreamingAssets(path, (jsonString) =>
        {
            if (!string.IsNullOrEmpty(jsonString))
            {
                BeatSaberMapDataSolo mapData = JsonUtility.FromJson<BeatSaberMapDataSolo>(jsonString);
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

    private IEnumerator SpawnBlocks(BeatSaberMapDataSolo mapData)
    {
        float startTime = Time.time;

        foreach (BeatSaberBlockDataSolo blockData in mapData._notes)
        {
            float targetTime = (blockData._time / bpm) * 60;


            float timeToWait = targetTime - (Time.time - startTime) + noteJumpStartBeatOffset;

            if (timeToWait > 0)
            {
                yield return new WaitForSeconds(timeToWait);
            }

            SpawnBlock(blockData);
        }
    }

    private void SpawnBlock(BeatSaberBlockDataSolo blockData)
    {
        Debug.Log("SPAWN " + blockData._time + "-" + blockData._type);

        GameObject blockPrefab = null;

        switch (blockData._type)
        {
            case (int)BeatSaberBlockType.Red:
                blockPrefab = redBlockPrefab;
                break;
            case (int)BeatSaberBlockType.Blue:
                blockPrefab = blueBlockPrefab;
                break;
            case (int)BeatSaberBlockType.Bomb:
                blockPrefab = bombBlockPrefab;
                break;
            default:
                Debug.LogError("Unknown block type: " + blockData._type);
                return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.x += blockData._lineIndex * horizontalSpacing;
        spawnPosition.y += blockData._lineLayer * verticalSpacing;

        GameObject blockInstance = Instantiate(blockPrefab, spawnPosition, Quaternion.identity);

        BlockControllerSolo blockController = blockInstance.GetComponent<BlockControllerSolo>();
        blockController.Initialize(blockData);
    }

    [System.Serializable]
    public class BeatSaberMapDataSolo
    {
        public BeatSaberBlockDataSolo[] _notes;
    }

    [System.Serializable]
    public class BeatSaberBlockDataSolo
    {
        public float _time;
        public int _lineIndex;
        public int _lineLayer;
        public int _type;
        public int _cutDirection;
    }


    public enum BeatSaberBlockType
    {
        Red, Blue, Bomb
    };
}

