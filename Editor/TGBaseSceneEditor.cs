using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[CustomEditor( typeof( TGBaseScene ), true )]
public class TGBaseSceneEditor: Editor {
    private TGBaseScene m_scene;

    private void OnEnable() {
        m_scene = target as TGBaseScene;
    }

    public override void OnInspectorGUI() {
        if( !Application.isPlaying ) {
            if( m_scene.SceneName == string.Empty ) {
                EditorGUILayout.HelpBox( "Please firstly save the scenes", MessageType.Error );
            }
            else {
                var scnData = SceneData.GetBySceneName( m_scene.SceneName );

                var sceneExist = scnData != null;

                GUI.color = ( sceneExist ) ? Color.white : new Color( 1f, 0.5f, 0.3f );

                if( GUILayout.Button( "Manage Setting Data" ) ) {
                    CreateScenePopupWindow.ShowWindow( m_scene );
                }

                GUI.color = Color.white;

                var deviceType = SceneDetail.GetBySceneName( m_scene.SceneName ).deviceType;
                EditorGUILayout.LabelField( "Device Type", deviceType );
            }
        }

        base.OnInspectorGUI();
    }
}