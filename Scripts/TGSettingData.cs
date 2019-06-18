using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[System.Serializable]
public struct SceneDetail {
    public string sceneName;
    public string deviceType;
    public bool disableHeatmap;

    public static SceneDetail GetBySceneName( string _sceneName ) {
        var setting = TGSettingData.GetInstance();

        if( setting == null ) {
            return new SceneDetail();
        }

        foreach( var d in setting.sceneDatas ) {
            foreach( var sd in d.sceneDetails ) {
                if( sd.sceneName == _sceneName )
                    return sd;
            }
        }

        return new SceneDetail ();
    }

    public override string ToString() {
        string format  = "场景名称：{0}\n";
               format += "设备类型: {1}\n";
               format += "启用热图: {2}\n";

        return string.Format( format, 
                              sceneName, 
                              deviceType,
                              disableHeatmap );
    }

    public bool IsAcive {
        get {
            return !string.IsNullOrEmpty( sceneName ) &&
                   !string.IsNullOrEmpty( deviceType );
        }
    }

    public override int GetHashCode() {
        return sceneName.GetHashCode();
    }

    public override bool Equals(object obj) {
        if( obj.GetType() != typeof( SceneDetail ) )
            return false;

        var compare = ( SceneDetail ) obj;

        return sceneName == compare.sceneName;
    }
}

[System.Serializable]
public class SceneData {
    public string productName = string.Empty;
    public string gameNameCn = string.Empty;
    public List<SceneDetail> sceneDetails = new List<SceneDetail>();

    public static SceneData GetBySceneName( string _sceneName ) {
        var setting = TGSettingData.GetInstance();

        if( setting == null ) {
            return null;
        }

        foreach( var d in setting.sceneDatas ) {
            foreach( var sd in d.sceneDetails ) {
                if( sd.sceneName == _sceneName )
                    return d;
            }
        }

        return null;
    }

    public override string ToString() {
        string format  = "Product Name: {0}\n";
               format += "中文名称: {1}\n";

        return string.Format( format, 
                              productName,
                              gameNameCn );
    }

    public bool IsActive {
        get { return !string.IsNullOrEmpty( productName ); }
    }

    public SceneDetail GetSceneDetail( string _deviceType ) {
        var retval = sceneDetails.FirstOrDefault( e => e.deviceType == _deviceType );
        if( !retval.IsAcive ) {
            throw new Exception ( string.Format( "{0}中找不到对应设备的场景：{1}", productName, _deviceType ) );
        }

        return retval;
    }

    public override int GetHashCode() {
        return productName.GetHashCode();
    }

    public override bool Equals(object obj) {
        if( obj.GetType() != typeof( SceneData ) )
            return false;

        var compare = ( SceneData ) obj;

        return productName == compare.productName ;
    }
}

[CreateAssetMenu( fileName = "SettingData", menuName = "TGFramework/SettingData", order = 0 )]
public class TGSettingData: ScriptableObject {
    private const string NO_PATIENCE = "SettingData中缺少对应的病患{0}设置";
    private const string NO_DEVICE_DEFAULT_SCENE = "SettingData中缺少对应设备{0}的默认场景"; 
    public List<SceneData> sceneDatas = new List<SceneData>();

    public static TGSettingData GetInstance() {
        var retval = Resources.Load<TGSettingData>( "SettingData" );

        if( retval == null )
            throw new Exception( "Resources文件夹里不存在SettingData" );

        return retval;
    }

    public SceneData GetSceneData( TGBaseScene _scene ) {
        foreach( var sd in sceneDatas ) {
            foreach( var d in sd.sceneDetails ) {
                if( d.sceneName == _scene.SceneName )
                    return sd;
            }
        }

        return new SceneData();
    }
}