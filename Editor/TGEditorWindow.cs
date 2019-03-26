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
    static TGEditorWindow myEditor;
    [MenuItem("Window/TGEditorWindow/CreateContollerScene")]

    static void SceneInit()
    {
        myEditor = (TGEditorWindow)EditorWindow.GetWindow(typeof(TGEditorWindow));
        myEditor.Show();
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
            CloseWindow();
        }
        GUILayout.EndVertical();
        GUIUtility.ExitGUI();
    }
   
    void CreatPrebaf()
    {
        Scene currScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        currScene.name = path;
        GameObject game = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/TGFramework/Prefabs/TGController.prefab");
        PrefabUtility.InstantiatePrefab(game);
        string sceneAssetPath = "Assets/_Project/Scenes";
        if (!Directory.Exists(sceneAssetPath))
        {
            Directory.CreateDirectory(sceneAssetPath);
            EditorSceneManager.SaveScene(currScene, sceneAssetPath+"/" + currScene.name + ".unity");
        }
        else
        {
            EditorSceneManager.SaveScene(currScene, "Assets/_Project/Scenes/" + currScene.name + ".unity");
        }
        
    }
    void CloseWindow()
    {
        Debug.Log("操作**CreateContollerScene**成功");
        myEditor.Close();
    }

}


public class CreateAsset : EditorWindow
{
    static CreateAsset myEditor;
    [MenuItem("Window/TGEditorWindow/CreateSettingData")]
    static void DataInit()
    {
        myEditor=(CreateAsset)EditorWindow.GetWindow(typeof(CreateAsset));
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("Create"))
        {
            CreateSettingData();
            CloseWindow();
        }
        GUILayout.EndVertical();
    }
    void CreateSettingData()
    {
        string projectPath = Application.dataPath + "/_Project/Resources";
        if (Directory.Exists(projectPath))
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
    void CloseWindow()
    {
        Debug.Log("操作**CreateSettingData**成功");
        myEditor.Close();
    }
}


public class RevisePlayerSetting : EditorWindow
{
    string ProductName;
    static RevisePlayerSetting myEditor;

    [MenuItem("Window/TGEditorWindow/RevisePlayerSetting")]
    static void Init()
    {
        myEditor=(RevisePlayerSetting)EditorWindow.GetWindow(typeof(RevisePlayerSetting));
    }
    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUI.skin.label.fontSize = 24;
        EditorGUILayout.LabelField("Product Name", EditorStyles.boldLabel);
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(300));
        ProductName = EditorGUI.TextField(rect, ProductName);
        if (GUILayout.Button("Revise"))
        {
            ReviseValue();
            CloseWindow();
        }
        GUILayout.EndVertical();
    }
    private void ReviseValue()
    {
        PlayerSettings.companyName = "Tuo Guan Technology";
        PlayerSettings.productName = ProductName;
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.HiddenByDefault;
    }
    void CloseWindow()
    {
        Debug.Log("操作**RevisePlayerSetting**成功");
        myEditor.Close();
    }
}
