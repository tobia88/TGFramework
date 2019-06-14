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
        string format  = "场景名称：{1}\n";
               format += "设备类型: {2}\n";
               format += "启用热图: {3}\n";
               format += "默认？: {4}\n";

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
        return sceneName.GetHashCode() + deviceType.GetHashCode();
    }

    public override bool Equals(object obj) {
        if( obj.GetType() != typeof( SceneDetail ) )
            return false;

        var compare = ( SceneDetail ) obj;

        return sceneName == compare.sceneName &&
               deviceType == compare.deviceType &&
               disableHeatmap == compare.disableHeatmap &&
               isDefault == compare.isDefault; 
 
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

    public bool CheckSceneExist( TGBaseScene _scene ) {
        foreach( var s in sceneDatas ) {
            if( s.patientType != _scene.patienceTarget )
                continue;

            if( s.sceneDetails.Contains( _scene.sceneDetail ))
                return true;
        }

        return false;
    }

    public SceneData GetSceneData( string _sceneName ) {
        foreach( var sd in sceneDatas ) {
            foreach( var d in sd.sceneDetails ) {
                if( d.sceneName == _sceneName )
                    return sd;
            }
        }

        return null;
    }

    public void SaveOrUpdateSceneDetail( SceneDetail _detail, PatientTypes _patience ) {
        // 为了避免病患目标改动而造成的重复添加
        // 先寻找一遍所有的sceneData，并把改场景中的SceneDetail删除掉
        // 之后会再重新添加
        foreach( var s in sceneDatas ) {
            if( s.sceneDetails.Contains( _detail ) )
                s.sceneDetails.Remove( _detail );
        }

        // 获取对应病患的容器
        var scnData = GetSceneData( _patience );

        if( scnData == null )
            throw new Exception( "找不到合适的病患目标，请在SettingData里建立一个" );
        
        scnData.sceneDetails.Add( _detail );
    }

    private SceneData GetSceneData( PatientTypes _patience ) {
        return sceneDatas.FirstOrDefault( s => s.patientType == _patience );
    }

}