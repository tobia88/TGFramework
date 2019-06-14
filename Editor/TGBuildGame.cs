using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class TGBuildGame: MonoBehaviour {
    public const string SCENE_PATH_FORMAT = "Assets/_Project/Scenes/{0}.unity";
    public const string FRAMEWORK_ROOT = "Assets/TGFramework/";
    
    [MenuItem( "TGFramework/Windows Build With Postprocess" )]
    public static void BuildGame() {
		try {
            // Get filename.
            string path = EditorUtility.SaveFolderPanel( "Choose Location of Built Game", "G:\\GameProjects\\Unity\\2018\\Builds", "" );

            if( string.IsNullOrEmpty( path ) )
                return;

            Build( path, true );
        }
		catch( Exception e ) {
			Debug.LogError( e );
		}
    }

    private static void Build( string _path, bool _openFolder ) {
        var settingData = TGSettingData.GetInstance();

        var sceneDatas = settingData.sceneDatas;

        if( sceneDatas == null || sceneDatas.Count == 0 ) {
            throw new Exception( "SettingData没有任何存储的场景" );
        }

        foreach( var d in sceneDatas ) {
            Build( d, _path, _openFolder );
        }
    }

    private static void Build( SceneData _sceneData, string _path, bool _openFolder ) {
        var details = _sceneData.sceneDetails;

        string[] levels = new string[details.Count + 1];

        levels[0] = string.Format( SCENE_PATH_FORMAT, "0_Controller" );

        for( int i = 0; i < details.Count; i++ ) {
            string scenePath = string.Format( SCENE_PATH_FORMAT, details[i].sceneName );
            levels[i+1] = scenePath;
            Debug.Log( "添加场景：" + scenePath );
        }

        string productName = _sceneData.productName;
        PlayerSettings.productName = productName;
        PlayerSettings.companyName = TGData.COMPANY_NAME;

        string targetFolder = _path + "/" + productName + "/";
        string targetExePath = targetFolder + "/" + productName + ".exe";

        Debug.Log( "编译地址：" + targetExePath );

        // 编译
        BuildPipeline.BuildPlayer( levels, targetExePath, BuildTarget.StandaloneWindows64, BuildOptions.None );

        // 输出配置表
        OutputConfigs( _sceneData, targetFolder );

        Debug.Log( "编译结束" );

        if( _openFolder )
            // 弹出目标目录
            Process.Start( targetFolder );
    }

    private static void OutputConfigs( SceneData _sceneData, string _targetFolder ) {
        CopyConfigToPath( FRAMEWORK_ROOT, _targetFolder, TGConfigs.MAIN_CONFIG_FILENAME );

        string srcConfigFolder = FRAMEWORK_ROOT + "Configs/";
        string dstConfigFolder = _targetFolder + "Configs/";

        Directory.CreateDirectory( dstConfigFolder );

        string[] outputConfigs = new string[]{
            TGConfigs.EVAL_SETTING_FILENAME,
            TGConfigs.KEY_INPUT_CONFIG_FILENAME
        };

        foreach( var c in outputConfigs ) {
            CopyConfigToPath( srcConfigFolder, dstConfigFolder, c );
        }

        // 制作Json为读取表
        LMFileWriter.Write( dstConfigFolder + TGConfigs.SCENE_SETTING_FILENAME, JsonUtility.ToJson( _sceneData, true ) );
    }

    private static void CopyConfigToPath( string _srcFolder, string _targetFolder, string _fileName ) {
        string src = _srcFolder + _fileName;
        string dst = _targetFolder + _fileName;

        Debug.Log( "复制" + src + "到" + dst );

        FileUtil.DeleteFileOrDirectory( dst );
        FileUtil.CopyFileOrDirectory( src, dst );
    }

    public static void Build() {
        string format = "{0}\\{1}\\{1}.exe";
        // 从命令行中获取发布路径
        string buildPath = string.Format( format, Environment.GetCommandLineArgs().Last(), Application.productName );
        Debug.Log( "Build Path: " + buildPath );

        Build( buildPath, false );
    }
}