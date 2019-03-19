﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TGMainGame : TGBaseBehaviour
{
    public string SceneName { get; private set; }
    public Dictionary<string, string> extraData;
    public TGBaseScene CurrentScene {get; private set;}


    public AsyncOperation asyncOperation;

    public override IEnumerator StartRoutine()
    {
        SceneName = m_controller.SceneName;

        Scene tmpScene = SceneManager.GetSceneByName(SceneName);


        if (!tmpScene.isLoaded)
        {
            asyncOperation = SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            while (!asyncOperation.isDone)
            {
                if (asyncOperation.progress >= 0.3f)
                {
                    float progress = asyncOperation.progress - 0.3f;
                    m_controller.ProgressValue = progress + 0.3f;
                }

                yield return null;
            }
        }


        CurrentScene = SetSceneActive(SceneName);

        if (CurrentScene == null)
        {
            m_controller.ErrorQuit("TGBaseScene doesn't found on scene " + this.SceneName);
        }
    }

    public override void ForceClose() {}

    public IEnumerator GameRoutine()
    {
        CurrentScene.Init();
        CurrentScene.OnStart();

        while (CurrentScene.isActive)
        {
            CurrentScene.OnUpdate();
            m_controller.inputSetting.OnUpdate();
            yield return 1;
        }

        extraData = CurrentScene.extraData;
    }

    public override IEnumerator EndRoutine()
    {
        if (CurrentScene == null)
            yield break;

        yield return StartCoroutine(CurrentScene.PreUnloadScene());
        yield return SceneManager.UnloadSceneAsync(SceneName);
    }


    private TGBaseScene SetSceneActive(string sceneName)
    {
        var tmpScene = SceneManager.GetSceneByName(SceneName);
        SceneManager.SetActiveScene(tmpScene);

        return tmpScene.GetComponent<TGBaseScene>();
    }

}