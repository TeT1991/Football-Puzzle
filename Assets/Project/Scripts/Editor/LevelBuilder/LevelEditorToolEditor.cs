using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelEditorTool tool = (LevelEditorTool)target;


        if(GUILayout.Button("Create Level"))
        {
            tool.CreateLevel();
        }

        if (GUILayout.Button("Clear Level"))
        {
            tool.ClearLevel();
        }

        if (GUILayout.Button("Start placing character"))
        {
            tool.StartPlacingCharacters(); 
        }

        if (GUILayout.Button("Stop placing character"))
        {
            tool.StopPlacingCharacters();
        }
    }
}
