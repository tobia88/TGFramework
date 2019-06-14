using System;
using System.Collections.Generic;
using UnityEngine;

public static class TGData {
    public const string SCORE_KEY = "�÷�";
    public const string MAIN_SCREENSHOT_KEY = "ͼƬ";
    public const string COMPANY_NAME = "Tuo Guan Technology";

    public static DateTime startTime;
    public static DateTime endTime;
    public static Dictionary<string, string> extraData = new Dictionary<string, string>();
    // public static SceneData curExportData;
    public static SceneData SceneData { get; private set; }
    public static SceneDetail SceneDetail { get; private set; }
    public static PatientTypes PatientTarget { get { return SceneData.patientType; } }
    public static KeyPortData KeyPortData { get; private set; }
    public static EvalData evalData;
    public static string GameNameCn { get; private set; }
    public static string SceneName { get; private set; }
    public static bool disableHeatmap;
    public static bool IsTesting { get; private set; }
    public static string DeviceName { get; private set; }
    public static string DeviceType { get { return KeyPortData.type; } }

    public static void SaveScreenshot( string _fileName ) {
        var key = MAIN_SCREENSHOT_KEY;

        if( extraData.ContainsKey( key ) ) {
            extraData[key] += "|" + _fileName;
        } else {
            extraData.Add( key, _fileName );
        }
    }

    public static void SaveScore( int score ) {
        if( extraData.ContainsKey( SCORE_KEY ) )
            extraData[SCORE_KEY] = score.ToString();
        
        else
            extraData.Add( SCORE_KEY, score.ToString() );
    }

    public static KeyPortData SetDevice( string _deviceName, bool _testing ) {
        IsTesting = _testing;
        Debug.Log( "�򿪲���: " + IsTesting );

        DeviceName = _deviceName;

        var config = KeyInputConfig.GetInstance();
        KeyPortData = config.GetKeyportData( DeviceName );

        Debug.Log( "�������ƣ�" + DeviceName );
        Debug.Log( "�豸���ͣ�" + DeviceType );

        return KeyPortData;
    }

    public static void SetSceneData( SceneData _sceneData ) {
        SceneData = _sceneData;
        GameNameCn = _sceneData.gameNameCn;

        SceneDetail = SceneData.GetSceneDetail( TGData.DeviceType );
        SceneName = SceneDetail.sceneName;
    }
}