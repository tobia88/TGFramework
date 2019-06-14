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
                m_settingData.sceneDatas.Add( new SceneData() );
            }

            if( GUILayout.Button( "删除")) {
                m_settingData.sceneDatas.RemoveAt( m_settingData.sceneDatas.Count - 1) ;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSceneData( int index ) {
        var data = m_settingData.sceneDatas[index];
        data.productName = EditorGUILayout.TextField( index + ".发布名称", data.productName );
        EditorGUI.indentLevel++;
        data.gameNameCn = EditorGUILayout.TextField( "中文名称", data.gameNameCn );
        data.patientType = (PatientTypes) EditorGUILayout.EnumPopup( "病患类型", data.patientType );
        EditorGUI.indentLevel--;

        Undo.RecordObject( m_settingData, "Update data" );
        m_settingData.sceneDatas[index] = data;
    }   

    private void EnsureDirectory() {
        if( Directory.Exists( TGPaths.ProjectResources ) )
            return;

        Directory.CreateDirectory( TGPaths.ProjectResources );
    }
}