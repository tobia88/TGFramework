using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( GameMng ) )]
public class TGBaseSceneEditor: Editor {
    private TGBaseScene m_scene;

    private void OnEnable() {
        m_scene = target as TGBaseScene;
        m_scene.sceneData = LinkSceneData();
    }

    public override void OnInspectorGUI() {
        if( !Application.isPlaying ) {
            if( m_scene.SceneName == string.Empty ) {
                EditorGUILayout.HelpBox( "Please firstly save the scenes", MessageType.Error );
            }
            else {
                var sceneExist = m_scene.sceneData != null &&
                                 m_scene.sceneData.sceneDetails.Contains( m_scene.sceneDetail );

                GUI.color = ( sceneExist ) ? Color.white : new Color( 1f, 0.5f, 0.3f );

                if( GUILayout.Button( "Manage Setting Data" ) ) {
                    CreateScenePopupWindow.ShowWindow( m_scene );
                }

                GUI.color = Color.white;
            }
        }

        base.OnInspectorGUI();
    }

    private SceneData LinkSceneData() {
        if( m_scene.sceneData == null )
            return null;

        var settingData = TGSettingData.GetInstance();

        return settingData.sceneDatas.FirstOrDefault( d => d.Equals( m_scene.sceneData ) );
    }
}