using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( GameMng ) )]
public class TGBaseSceneEditor: Editor {
    private TGBaseScene _scene;

    private void OnEnable() {
        CreateScenePopupWindow.onConfirm += OnConfirmHandle;
        _scene = target as TGBaseScene;
    }

    private void OnDisable() {
        CreateScenePopupWindow.onConfirm -= OnConfirmHandle;
    }

    private void OnConfirmHandle( bool result ) {
        if( result ) {
            SaveOrUpdateSceneDetail();
        }
    }

    private void SaveOrUpdateSceneDetail() {
        var settingData = TGSettingData.GetInstance();
        settingData.SaveOrUpdateSceneDetail( _scene.sceneDetail, _scene.patienceTarget );
    }

    public override void OnInspectorGUI() {
        if( !Application.isPlaying ) {
            var settingData = TGSettingData.GetInstance();
            var sceneExist = settingData.CheckSceneExist( _scene );

            _scene.sceneDetail.sceneName = _scene.SceneName;

            GUI.color = ( sceneExist ) ? Color.white : Color.red;

            if( GUILayout.Button( "Create or Update" ) ) {
                CreateScenePopupWindow.ShowWindow( _scene );
            }

            GUI.color = Color.white;
        }

        base.OnInspectorGUI();
    }
}