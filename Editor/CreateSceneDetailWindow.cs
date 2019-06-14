
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class CreateScenePopupWindow : EditorWindow {
    private PatientTarget _prevPatientTarget;
    private SceneDetail _prevSceneDetail;

    public SceneData sceneData;
    public static event Action<bool> onConfirm; 
    public const int HEIGHT = 100;
    public const int WIDTH = 300;

    public static void ShowWindow( TGBaseScene _scene ) {
        var window = EditorWindow.GetWindow<CreateScenePopupWindow>();
        window.ShowPopup();

        window.titleContent = new GUIContent( "添加病患类型" );
        window.position = new Rect( 600, Screen.height/2 - HEIGHT/2, WIDTH, HEIGHT );

        window.Init( _scene );
    }

    public void Init( TGBaseScene _scene ) {
        sceneData = new SceneData();
        sceneData.patienceTarget = _scene.patienceTarget;

        Input.imeCompositionMode = IMECompositionMode.On;
    }

    private void OnGUI() {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField( "病患类型", sceneData.patienceTarget.ToString() );
        sceneData.productName = EditorGUILayout.TextField( "Product Name", sceneData.productName );
        sceneData.gameNameCn = EditorGUILayout.TextField( "中文名字", sceneData.gameNameCn );

        EditorGUILayout.BeginHorizontal();

        if( GUILayout.Button("Confirm") ) {
            var setting = TGSettingData.GetInstance();
            setting.sceneDatas.Add( sceneData );
            onConfirm( true );
            Close();
        }

        if( GUILayout.Button( "Cancel" ) ) {
            onConfirm( false );
            Close();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}