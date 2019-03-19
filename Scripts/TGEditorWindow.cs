using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class TGEditorWindow : EditorWindow
{


    string path;
    [MenuItem("Window/TGEditorWindow/CreateContollerScene")]
    static void SceneInit()
    {
        EditorWindow.GetWindow(typeof(TGEditorWindow));
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 24;
        EditorGUILayout.LabelField("CreaterSceneName", EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        path = EditorGUI.TextField(rect, path);
        if (GUILayout.Button("Create"))
        {
            CreatPrebaf();
        }
        GUILayout.EndVertical();
    }
    void CreatPrebaf()
    {
        Scene currScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        currScene.name = path;
        GameObject game = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TGFramework/Prefabs/TGController.prefab");
        PrefabUtility.InstantiatePrefab(game);
        EditorSceneManager.SaveScene(currScene, "Assets/_Project/Scenes/" + currScene.name + ".unity");
    }


}


public class CreateAsset : EditorWindow
{
    [MenuItem("Window/TGEditorWindow/CreateSettingData")]
    static void DataInit()
    {
        EditorWindow.GetWindow(typeof(CreateAsset));
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Create"))
        {
            CreateSettingData();
        }
        GUILayout.EndVertical();
    }
    void CreateSettingData()
    {
        string projectPath = Application.dataPath + "/_Project/Resources";
        if (!Directory.Exists(projectPath))
        {
            if (AssetDatabase.FindAssets("Assets/_Project/Resources/SettingData.asset") == null)
            {
                TGSettingData data = ScriptableObject.CreateInstance<TGSettingData>();
                AssetDatabase.CreateAsset(data, "Assets/_Project/Resources/SettingData.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                return;
            }
        }
        else
        {
            Directory.CreateDirectory(projectPath);
            TGSettingData data = ScriptableObject.CreateInstance<TGSettingData>();
            AssetDatabase.CreateAsset(data, "Assets/_Project/Resources/SettingData.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}


public class RevisePlayerSetting : EditorWindow
{
    string name;
    [MenuItem("Window/TGEditorWindow/RevisePlayerSetting")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(RevisePlayerSetting));
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 24;
        EditorGUILayout.LabelField("Product Name", EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        name = EditorGUI.TextField(rect, name);
        if (GUILayout.Button("Revise"))
        {
            ReviseValue();
        }
        GUILayout.EndVertical();
    }
    private void ReviseValue()
    {
        PlayerSettings.companyName = "Tuo Guan Technology";
        PlayerSettings.productName = name;
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.HiddenByDefault;
    }
}
