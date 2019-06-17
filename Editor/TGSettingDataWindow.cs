using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public class TGSettingDataWindow : EditorWindow {
    private TGSettingData m_settingData;

    [MenuItem( "TGFramework/Preferences" )]
    public static void ShowWindow() {
        var window = EditorWindow.GetWindow<TGSettingDataWindow>();
        window.titleContent = new GUIContent( "Setting Data" );

        Input.imeCompositionMode = IMECompositionMode.On;
    }
    
    private void OnGUI() {
        m_settingData = AssetDatabase.LoadAssetAtPath<TGSettingData>( TGPaths.SettingData );

        if( m_settingData == null ) {
            // SettingData不存在，提示创建Setting Data
            if( GUILayout.Button( "初始化") ) {
                EnsureDirectory();
                var data = ScriptableObject.CreateInstance<TGSettingData>();

                AssetDatabase.CreateAsset( data, TGPaths.SettingData );
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
        else {
            for( int i = 0; i < m_settingData.sceneDatas.Count; i++ ) {
                DrawSceneData( i );
                TGEditorUtility.DrawUILine( Color.grey, 1, 20 );
            }

            EditorGUILayout.BeginHorizontal();
            if( GUILayout.Button( "创建" ) ) {
                Undo.RecordObject( m_settingData, "Create new Scene Data" );
                m_settingData.sceneDatas.Add( new SceneData() );
            }

            if( GUILayout.Button( "删除")) {
                Undo.RecordObject( m_settingData, "Remove Last Scene Data" );
                if( m_settingData.sceneDatas != null && m_settingData.sceneDatas.Count > 0 )
                    m_settingData.sceneDatas.RemoveAt( m_settingData.sceneDatas.Count - 1 );
            }
            EditorGUILayout.EndHorizontal();

            if( GUILayout.Button( "清理" ) ) {
                ClearNullScene();
            }
        }
    }

    private void DrawSceneData( int _index ) {
        var data = m_settingData.sceneDatas[_index];
        data.productName = EditorGUILayout.TextField( _index + ".发布名称", data.productName );
        EditorGUI.indentLevel++;
        data.gameNameCn = EditorGUILayout.TextField( "中文名称", data.gameNameCn );
        data.patientType = (PatientTypes) EditorGUILayout.EnumPopup( "病患类型", data.patientType );

        // 列出所有场景
        EditorGUILayout.LabelField( "附加场景：");
        EditorGUI.indentLevel++;
        data.sceneDetails.ForEach( sd => DrawSceneDetail( sd ) );
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;

        Undo.RecordObject( m_settingData, "Update data" );
        m_settingData.sceneDatas[_index] = data;
    }   

    private void DrawSceneDetail( SceneDetail _sc ) {
        Debug.Log( _sc.sceneName );
        var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>( TGPaths.FullScenePath( _sc.sceneName ) );
        if( asset == null ) {
            EditorGUILayout.HelpBox( "Please make sure scene has saved to Assets/_Projects/Scenes Directory", MessageType.Warning );
        }
        EditorGUILayout.ObjectField( asset, typeof( SceneAsset ), false );
    }

    private void EnsureDirectory() {
        if( Directory.Exists( TGPaths.ProjectResources ) )
            return;

        Directory.CreateDirectory( TGPaths.ProjectResources );
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
    }
}