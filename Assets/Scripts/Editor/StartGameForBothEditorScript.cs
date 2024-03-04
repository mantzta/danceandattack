using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StartGameForBoth))]
public class StartGameForBothEditorScript : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox("This script starts the game", MessageType.Info);

        StartGameForBoth startGameForBoth = (StartGameForBoth)target;

        if (GUILayout.Button("Start Game"))
        {
            startGameForBoth.SetPlayerToReady();
        }

        if (GUILayout.Button("Start Solo Game"))
        {
            startGameForBoth.SetSoloGame();
        }
    }

}