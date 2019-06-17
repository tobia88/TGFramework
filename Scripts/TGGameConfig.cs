using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum PatientTypes {
    Adult = 0,
    Child = 1
}

public class EvalDataGroup {
    public EvalData[] infos;
}

public class TGGameConfig: TGBaseManager {
    public static TGGameConfig Instance { get; private set; }
    public const string SECTION_NAME = "PZConf";

    private static INIParser m_iniParser;

    public override void Init(TGController _controller) {
        Instance = this;

        Screen.SetResolution( 1920, 1080, true, 60 );

        base.Init( _controller );
    }

    public override IEnumerator StartRoutine() {
        try {
            InitParser();
            LoadInputConfig();
            LoadEvaluationSetting();
            LoadSceneData();

            m_controller.ProgressValue += 0.1f;
        }
        catch( Exception e ) {
            m_controller.ErrorQuit( e.ToString() );
        }

        yield return 1;
    }

    private void LoadEvaluationSetting() {
        string eval = TGPaths.EvalSetting;

        // 读取3D传感器的配置
        if( !string.IsNullOrEmpty( eval ) ) {
            var group = LMFileWriter.ReadJSON<EvalDataGroup>( eval );

            if( group != null ) {
                string cnTitle = GetValue( "体侧", string.Empty );
                TGData.evalData = GetConfigDataFromTitle( group, cnTitle );
            }
        } else {
            Debug.LogWarning( eval + "Has not found" );
        }
    }

    public override void ForceClose() {
        Close();
    }

    public override IEnumerator EndRoutine() {
        Close();
        yield return 1;
    }

    private EvalData GetConfigDataFromTitle( EvalDataGroup group, string cnTitle ) {
        return group.infos.FirstOrDefault( d => d.cnTitle == cnTitle );
    }

    public static string GetValue( string key, string defaultValue ) {
        return m_iniParser.ReadValue( SECTION_NAME, key, defaultValue );
    }

    public static int GetValue( string key, int defaultValue ) {
        return m_iniParser.ReadValue( SECTION_NAME, key, defaultValue );
    }

    public static float GetValue( string key, float defaultValue ) {
        return ( float )m_iniParser.ReadValue( SECTION_NAME, key, defaultValue );
    }

    public void Close() {
        if( m_iniParser != null )
            m_iniParser.Close();
    }

    public void LoadSceneData() {
        #if UNITY_EDITOR
            var settingData = TGSettingData.GetInstance();
            var scn = GameObject.FindObjectOfType<TGBaseScene> ();
            
            SceneData sceneData = new SceneData();

            if( scn != null ) {
                sceneData = settingData.GetSceneData( scn );
            }
            else {
                sceneData = settingData.sceneDatas[0];
            }

        #else
            var sceneData = LMFileWriter.ReadJSON<SceneData>( TGPaths.SceneData );
        #endif   

        TGData.SetSceneData( sceneData );
    }

    private void LoadInputConfig() {
        bool testing = false;
        string deviceName = string.Empty;

        var scn = GameObject.FindObjectOfType<TGBaseScene>();
        deviceName = string.Empty;

        if( scn != null ) {
            // 获取设置中的输入类型
            deviceName = "触屏控制";
            testing = scn.isTesting;
        } else {
            // 获取game.txt中的数据
            testing = TGGameConfig.GetValue( "测试", 0 ) == 1;
            deviceName = TGGameConfig.GetValue( "训练器材", string.Empty );
        }

        TGData.SetDevice( deviceName, testing );
    }

    private void InitParser() {
        string path = TGPaths.MainGameConfig;

        m_iniParser = new INIParser();
        m_iniParser.Open( path );

        if( string.IsNullOrEmpty( m_iniParser.iniString ) ) {
            m_controller.ErrorQuit( "Config is NULL! Path = " + path );
            return;
        }
    }
}