using UnityEditor;
using UnityEngine;

public class LevelEditorAssetUtility
{
    private const string DefaultFolderPath = "Assets/Project/Data/Levels";
    private const string DefaultAssetName = "NewLevelDefinition.asset";

    public LevelDefinition Save(LevelDefinition levelDefinition)
    {
        if (levelDefinition == null)
        {
            Debug.LogWarning("LevelDefinition is null. Nothing to save.");
            return null;
        }

        string assetPath = AssetDatabase.GetAssetPath(levelDefinition);

        if (string.IsNullOrEmpty(assetPath))
        {
            return SaveAsNew(levelDefinition);
        }

        EditorUtility.SetDirty(levelDefinition);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Level saved: {assetPath}");

        return levelDefinition;
    }

    public LevelDefinition SaveAsNew(LevelDefinition source)
    {
        if (source == null)
        {
            Debug.LogWarning("LevelDefinition is null. Can't save as new.");
            return null;
        }

        EnsureFolderExists(DefaultFolderPath);

        string path = AssetDatabase.GenerateUniqueAssetPath(
            $"{DefaultFolderPath}/{DefaultAssetName}");

        LevelDefinition newLevelDefinition = ScriptableObject.CreateInstance<LevelDefinition>();
        newLevelDefinition.CopyFrom(source);

        AssetDatabase.CreateAsset(newLevelDefinition, path);
        EditorUtility.SetDirty(newLevelDefinition);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"New level created: {path}");

        return newLevelDefinition;
    }

    // Оставил старый метод с твоим старым названием,
    // чтобы не сломались возможные вызовы из старого редактора.
    public LevelDefinition CreateLevelDefinitonAsset(LevelData levelData)
    {
        LevelDefinition levelDefinition = ScriptableObject.CreateInstance<LevelDefinition>();
        levelDefinition.UpdateData(levelData);

        return SaveAsNew(levelDefinition);
    }

    private void EnsureFolderExists(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] folders = folderPath.Split('/');

        if (folders.Length == 0 || folders[0] != "Assets")
        {
            Debug.LogWarning($"Invalid folder path: {folderPath}");
            return;
        }

        string currentPath = "Assets";

        for (int i = 1; i < folders.Length; i++)
        {
            string nextPath = $"{currentPath}/{folders[i]}";

            if (AssetDatabase.IsValidFolder(nextPath) == false)
            {
                AssetDatabase.CreateFolder(currentPath, folders[i]);
            }

            currentPath = nextPath;
        }
    }
}