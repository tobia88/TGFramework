using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGMainGame: TGBaseBehaviour {
    public string SceneName { get; private set; }
    public TGBaseScene CurrentScene { get; private set; }

    public AsyncOperation asyncOperation;

    public override IEnumerator StartRoutine() {
        SceneName = m_controller.SceneName;

        Scene tmpScene = SceneManager.GetSceneByName( SceneName );

        if( !tmpScene.isLoaded ) {
            yield return StartCoroutine( LoadSceneRoutine() );
        }

        CurrentScene = SetSceneActive( SceneName );

        if( CurrentScene == null ) {
            m_controller.ErrorQuit( "TGBaseScene doesn't found on scene " + this.SceneName );
        }
    }

    private IEnumerator LoadSceneRoutine() {

        yield return StartCoroutine( ClearLoadingScene() );

        Debug.Log( "读取场景：" + SceneName );
        asyncOperation = SceneManager.LoadSceneAsync( SceneName, LoadSceneMode.Additive );

        float remainProg = 1f - m_controller.ProgressValue;

        float lastProgress = 0f;

        while( !asyncOperation.isDone ) {
            float d = asyncOperation.progress - lastProgress;
            m_controller.ProgressValue += d * remainProg;
            lastProgress = asyncOperation.progress;

            yield return null;
        }
    }

    private IEnumerator ClearLoadingScene() {
        for( int i = SceneManager.sceneCount - 1; i >= 0; i-- ) {
            var scn = SceneManager.GetSceneAt( i );

            if( scn == gameObject.scene )
                continue;

            Debug.Log( "Clearing Scene: " + scn.name );

            yield return SceneManager.UnloadSceneAsync( scn );
        }
    }

    public override void ForceClose() {
        if( CurrentScene != null )
            CurrentScene.ForceClose();
    }

    public IEnumerator GameRoutine() {
        // 记录开始时间
        TGData.startTime = DateTime.Now;

        // 初始化场景
        CurrentScene.Init();

        // 配置输入设置
        m_inputSetting.OnGameStart();

        // 游戏场景开始
        CurrentScene.OnStart();

        // 游戏场景循环
        while( CurrentScene.isActive ) {
            CurrentScene.OnUpdate();
            m_controller.inputSetting.OnUpdate();
            yield return 1;
        }

        // 记录结束时间
        TGData.endTime = DateTime.Now;
    }

    public override IEnumerator EndRoutine() {
        if( CurrentScene == null )
            yield break;

        yield return StartCoroutine( CurrentScene.PreUnloadScene() );
        yield return SceneManager.UnloadSceneAsync( SceneName );
    }

    private TGBaseScene SetSceneActive( string sceneName ) {
        var tmpScene = SceneManager.GetSceneByName( SceneName );
        SceneManager.SetActiveScene( tmpScene );

        return tmpScene.GetComponent<TGBaseScene>();
    }

}