using UnityEditor;
using UnityEngine;

public class LevelEditorAssetUtility 
{
    private string _path = "Assets/Project/Data/Levels/Test/NewLevelDefinition.asset";

    public LevelDefinition CreateLevelDefinitonAsset(LevelData levelData)
    {
        LevelDefinition levelDefinition = ScriptableObject.CreateInstance<LevelDefinition>();
        levelDefinition.UpdateData(levelData);
        string path = AssetDatabase.GenerateUniqueAssetPath(_path);
        AssetDatabase.CreateAsset(levelDefinition, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return levelDefinition;
    }

}

