using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveLogs : MonoBehaviour
{
    private static string _FilePath;

    public static void SaveLogData(LogData data)
    {
        // This is only null when the game starts
        if (_FilePath == null)
        {
            CreateNewFile();
        }

        string dataString = JsonUtility.ToJson(data);
        File.WriteAllText(_FilePath, dataString);
    }

    public static void CreateNewFile()
    {
        string date = System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss");
        _FilePath = Application.persistentDataPath + $"/PlayerLogData_{date}.json";

        File.WriteAllText(_FilePath, "Player did not play yet");
    }
}

[System.Serializable]
public class LogData
{
    public string Player;
    public int Score;
    public List<int> Streaks;
    public int Mistakes;
    public List<int> MistakeStreaks;
    public List<float> Adaptations;


    public LogData(string player, int score, List<int> streaks, int mistakes, List<int> mistakeStreaks, List<float> adaptations)
    {
        Player = player;
        Score = score;
        Streaks = streaks;
        Mistakes = mistakes;
        MistakeStreaks = mistakeStreaks;
        Adaptations = adaptations;
    }
}
