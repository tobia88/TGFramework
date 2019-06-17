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
    public bool isDefault;

    public override string ToString() {
        string format  = "场景名称：{0}\n";
               format += "设备类型: {1}\n";
               format += "启用热图: {2}\n";
               format += "默认？: {3}\n";

        return string.Format( format, 
                              sceneName, 
                              deviceType,
                              disableHeatmap,
                              isDefault );
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
    public PatientTypes patientType = PatientTypes.Adult;
    public List<SceneDetail> sceneDetails = new List<SceneDetail>();

    public override string ToString() {
        string format  = "Product Name: {0}\n";
               format += "中文名称: {1}\n";
               format += "病人类型：{2}\n";

        return string.Format( format, 
                              productName,
                              gameNameCn, 
                              patientType );
    }

    public bool IsActive {
        get { return !string.IsNullOrEmpty( productName ); }
    }

    public SceneDetail GetSceneDetail( string _deviceType ) {
        var retval = sceneDetails.FirstOrDefault( e => e.deviceType == _deviceType );
        if( !retval.IsAcive ) {
            Debug.LogWarning( "SceneDetails中找不到对应设备的场景：" + _deviceType );
            retval = sceneDetails.FirstOrDefault( e => e.isDefault );

            if( !retval.IsAcive )
                throw new Exception( "SceneDetails中没有设置默认场景" );
        }

        return retval;
    }

    public override int GetHashCode() {
        return productName.GetHashCode() + gameNameCn.GetHashCode() + patientType.GetHashCode();
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