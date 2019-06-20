using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class TGSettingDataWindow : EditorWindow {
    private TGSettingData m_settingData;
    private SceneAsset m_controllerScene;

    [MenuItem( "TGFramework/Preferences" )]
    public static void ShowWindow() {
        var window = EditorWindow.GetWindow<TGSettingDataWindow>();
        window.titleContent = new GUIContent( "Setting Data" );

        Input.imeCompositionMode = IMECompositionMode.On;
    }
    
    private void OnGUI() {
        m_controllerScene = AssetDatabase.LoadAssetAtPath<SceneAsset>( TGPaths.ControllerScene );
        m_settingData = AssetDatabase.LoadAssetAtPath<TGSettingData>( TGPaths.SettingData );

        if( m_settingData == null || m_controllerScene == null ) {
            EditorGUILayout.HelpBox( "Setting Data or Controller Scene doesn't Exist", MessageType.Error );

            // SettingData不存在，提示创建Setting Data
            if( GUILayout.Button( "初始化") ) {
                EnsureDirectories();

                if( m_settingData == null ) 
                    m_settingData = CreateSettingData();

                if( m_controllerScene == null )
                    m_controllerScene = CreateControllerScene();
            }
        }
        else {
            EditorGUILayout.ObjectField( "Setting Data", m_settingData, typeof( TGSettingData ), false );
            EditorGUILayout.ObjectField( "Controller Scene", m_controllerScene, typeof( SceneAsset ), false );

            TGEditorUtility.DrawUILine( Color.gray, 1, 20 );

            for( int i = 0; i < m_settingData.sceneDatas.Count; i++ ) {
                DrawSceneData( i );
                TGEditorUtility.DrawUILine( Color.grey, 1, 20 );
            }

            EditorGUILayout.BeginHorizontal();
            if( GUILayout.Button( "Create" ) ) {
                Undo.RecordObject( m_settingData, "Create new Scene Data" );
                m_settingData.sceneDatas.Add( new SceneData() );
                EditorUtility.SetDirty( m_settingData );
            }

            if( GUILayout.Button( "Remove Last")) {
                Undo.RecordObject( m_settingData, "Remove Last Scene Data" );
                if( m_settingData.sceneDatas != null && m_settingData.sceneDatas.Count > 0 )
                    m_settingData.sceneDatas.RemoveAt( m_settingData.sceneDatas.Count - 1 );
                EditorUtility.SetDirty( m_settingData );
            }
            EditorGUILayout.EndHorizontal();

            if( GUILayout.Button( "Clear Null Scenes" ) ) {
                ClearNullScene();
            }

            var c = GUI.color;
            GUI.color = Color.green;
            if( GUILayout.Button( "Build") ) {
                TGBuildGame.BuildGame();
            }
            GUI.color = c;
        }
    }

    private void DrawSceneData( int _index ) {
        var data = m_settingData.sceneDatas[_index];

        string newName = EditorGUILayout.TextField( _index + ". Name", data.dataName );
        if( data.dataName != newName ) {
            Undo.RecordObject( m_settingData, "Update Data Name" );
            data.dataName = newName;
            EditorUtility.SetDirty( m_settingData );
        }

        EditorGUI.indentLevel++;

        string newProductName = EditorGUILayout.TextField( "Product Name", data.productName );

        // 发布名称
        if( data.productName != newProductName ) {
            Undo.RecordObject( m_settingData, "Update Product Name" );
            data.productName = newProductName;
            EditorUtility.SetDirty( m_settingData );
        }

        // 中文名称
        string newCnName = EditorGUILayout.TextField( "Chinese Name", data.gameNameCn );

        if( data.gameNameCn != newCnName ) {
            Undo.RecordObject( m_settingData, "Update Chinese Name" );
            data.gameNameCn = newCnName;
            EditorUtility.SetDirty( m_settingData );
        }

        // 编译路径
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField( "Build Path", data.buildPath );
        if( GUILayout.Button( "Choose..", GUILayout.MaxWidth( 100 ))) {
            string path = EditorUtility.SaveFolderPanel( "Choose Location of Built Game", "G:\\GameProjects\\Unity\\2018\\Builds", "" );
            if( !string.IsNullOrEmpty( path ) ){
                Undo.RecordObject( m_settingData, "Update Product Name" );
                data.buildPath = path;
                EditorUtility.SetDirty( m_settingData );
            }
        }

        EditorGUILayout.EndHorizontal();
        // 列出所有场景
        EditorGUILayout.LabelField( "Packed Scenes:");
        EditorGUI.indentLevel++;
        data.sceneDetails.ForEach( sd => DrawSceneDetail( sd ) );
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }   

    private void DrawSceneDetail( SceneDetail _sc ) {
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>( TGPaths.FullScenePath( _sc.sceneName ) );
        if( asset == null ) {
            EditorGUILayout.HelpBox( "Please make sure scene has saved to Assets/_Projects/Scenes Directory", MessageType.Warning );
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField( asset, typeof( SceneAsset ), false, GUILayout.Width( 350 ), GUILayout.MinWidth( 150 ) );
        EditorGUILayout.LabelField( _sc.deviceType, GUILayout.Width( 100) );
        EditorGUILayout.EndHorizontal();
    }

    private void EnsureDirectories() {
        if( !Directory.Exists( TGPaths.ProjectResources ) )
            Directory.CreateDirectory( TGPaths.ProjectResources );

        if( !Directory.Exists( TGPaths.ProjectScenes ) )
            Directory.CreateDirectory( TGPaths.ProjectScenes );
    }

    // 清理没用的场景
    private void ClearNullScene() {
        var sceneDatas = m_settingData.sceneDatas;
        for( int i = sceneDatas.Count - 1; i >= 0; i-- ) {
            var sceneDetails = sceneDatas[i].sceneDetails;
            for( int j = sceneDetails.Count - 1; j >= 0; j-- ) {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>( TGPaths.FullScenePath( sceneDetails[j].sceneName) );
                
                // 找不到该场景则清除该场景
                if( asset == null )
                    sceneDatas[i].sceneDetails.RemoveAt( j );
            }
        }
        EditorUtility.SetDirty( m_settingData );
    }

    private TGSettingData CreateSettingData() {
        var data = ScriptableObject.CreateInstance<TGSettingData>();

        AssetDatabase.CreateAsset( data, TGPaths.SettingData );
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<TGSettingData>( TGPaths.SettingData );
    }

    // 初始化Controller场景
    private SceneAsset CreateControllerScene() {
        // 创建空场景
        Scene tmpScene = EditorSceneManager.NewScene( NewSceneSetup.EmptyScene, NewSceneMode.Single );
        tmpScene.name = TGController.SCENE_NAME;

        // 实例化Prefab到空场景
        TGController controllerPrefab = Resources.Load<TGController>( "TGController" );
        PrefabUtility.InstantiatePrefab( controllerPrefab );

        // 存储场景
        string path = TGPaths.ControllerScene;
        EditorSceneManager.SaveScene( tmpScene, path );

        // 设该选择对象为新建场景
        var retval = AssetDatabase.LoadAssetAtPath<SceneAsset>( path );
        Selection.activeObject = retval; 

        return retval;
    }
}