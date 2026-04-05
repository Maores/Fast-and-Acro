using UnityEngine;
using UnityEditor;

/// <summary>
/// Ensures required tags exist in the project's TagManager.
/// Called automatically by SceneBuilder, or manually via menu.
/// </summary>
public static class TagCreator
{
    [MenuItem("Tools/Fast and Acro/Ensure Tags Exist")]
    public static void EnsureTags()
    {
        AddTag("Player");
        AddTag("Obstacle");
        Debug.Log("[TagCreator] Tags verified: Player, Obstacle");
    }

    public static void AddTag(string tag)
    {
        // Check if tag already exists
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
                return;
        }

        // Open TagManager asset and add the tag
        SerializedObject tagManager =
            new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Find first empty slot or add new entry
        int index = -1;
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            SerializedProperty element = tagsProp.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(element.stringValue))
            {
                index = i;
                break;
            }
        }

        if (index == -1)
        {
            index = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(index);
        }

        tagsProp.GetArrayElementAtIndex(index).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
