
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class CreateScenePopupWindow : EditorWindow {
    private SceneData m_prevSceneData;
    private SceneDetail m_prevSceneDetail;
    private SceneData m_newSceneData;
    private SceneDetail m_newSceneDetail;
    private TGBaseScene m_scene;

    public const int HEIGHT = 150;
    public const int WIDTH = 300;

    public static void ShowWindow( TGBaseScene _scene ) {
        var window = EditorWindow.GetWindow<CreateScenePopupWindow>();
        window.ShowPopup();

        window.titleContent = new GUIContent( "场景信息" );
        window.position = new Rect( 600, Screen.height/2 - HEIGHT/2, WIDTH, HEIGHT );

        window.Init( _scene );
        Input.imeCompositionMode = IMECompositionMode.On;
    }

    public void Init( TGBaseScene _scene ) {
        m_scene = _scene;

        m_prevSceneData = m_newSceneData = SceneData.GetBySceneName( m_scene.SceneName );
        m_prevSceneDetail = m_newSceneDetail = SceneDetail.GetBySceneName( m_scene.SceneName );
    }


    private void OnGUI() {
        EditorGUILayout.BeginVertical();

        var settingData = TGSettingData.GetInstance();

        // 如果没有则提示到TGframework/Preferences里创建
        if( settingData.sceneDatas == null || settingData.sceneDatas.Count == 0 ) {
            EditorGUILayout.HelpBox( "没有任何的SceneData，请通过TGFramework/Preferences创建", MessageType.Error );
            return;
        }
        // 如果有则允许通过弹窗选择，并且列出该信息
        m_newSceneData = DrawSceneDataSelection();
        m_newSceneDetail = DrawSceneDetailSelection();

        EditorGUILayout.BeginHorizontal();

        if( GUILayout.Button( "Confirm" ) ) {
            var setting = TGSettingData.GetInstance();
            // 检测信息是否发生改变，如果发生改变则先把旧的信息移除
            if( m_prevSceneData != m_newSceneData && m_prevSceneData != null ) {
                m_prevSceneData.sceneDetails.Remove( m_prevSceneDetail );
            }

            // 如果该场景已经存在，则更新其数值
            // 反之则添加新数据到列表里
            if( m_newSceneData != null ) {
                var scnIndex = m_newSceneData.sceneDetails.IndexOf( m_newSceneDetail );
                if( scnIndex >= 0 ) {
                    m_newSceneData.sceneDetails[scnIndex] = m_newSceneDetail;
                } else {
                    m_newSceneData.sceneDetails.Add( m_newSceneDetail );
                }
            }

            EditorUtility.SetDirty( settingData );
            
            EditorWindow.GetWindow<TGSettingDataWindow>().Repaint();

            Close();
        }

        if( GUILayout.Button( "Cancel" ) ) {
            Close();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    private SceneData DrawSceneDataSelection() {
        var settingData = TGSettingData.GetInstance();
        string[] sceneDatas = settingData.sceneDatas.Select( d => d.productName ).ToArray();

        int[] optionValues = new int[sceneDatas.Length];
        for( int i = 0; i < optionValues.Length; i++ )
            optionValues[i] = i;

        int selectedIndex = settingData.sceneDatas.IndexOf( m_newSceneData );

        if( selectedIndex == -1 )
            selectedIndex = 0;

        selectedIndex = EditorGUILayout.IntPopup( "Setting Data", selectedIndex, sceneDatas, optionValues );

        return settingData.sceneDatas[selectedIndex];
    }

    private SceneDetail DrawSceneDetailSelection() {
        var retval = m_newSceneDetail;

        retval.sceneName = EditorGUILayout.TextField( "场景名称", m_scene.SceneName );
        retval.deviceType = EditorGUILayout.TextField( "设备类型", m_scene.deviceType );
        retval.disableHeatmap = EditorGUILayout.Toggle( "取消热图？", retval.disableHeatmap );
        retval.isDefault = EditorGUILayout.Toggle( "默认？", retval.isDefault );

        return retval;
    }
}