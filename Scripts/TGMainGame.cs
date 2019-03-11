using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TGMainGame : TGBaseBehaviour
{
    public string SceneName { get; private set; }
    public Dictionary<string, string> extraData;
    public TGBaseScene CurrentScene {get; private set;}

    public override IEnumerator StartRoutine()
    {
        SceneName = m_controller.SceneName;

        Scene tmpScene = SceneManager.GetSceneByName(SceneName);

        if (!tmpScene.isLoaded)
        {
            // FIXME: 插入读取场景
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
            // FIXME: 释放读取场景
        }

        Debug.Log("Scene Loaded: " + SceneName);

        CurrentScene = SetSceneActive(SceneName);

        if (CurrentScene != null)
        {
            yield return m_controller.StartCoroutine(GameRoutine(CurrentScene));
            yield return m_controller.StartCoroutine(UnloadScene(CurrentScene));

            Debug.Log("Main Game: Finished");
        }
        else
        {
            m_controller.ErrorQuit("TGBaseScene doesn't found on scene " + this.SceneName);
            yield break;
        }
    }

    private TGBaseScene SetSceneActive(string sceneName)
    {
        var tmpScene = SceneManager.GetSceneByName(SceneName);
        SceneManager.SetActiveScene(tmpScene);

        return tmpScene.GetComponent<TGBaseScene>();
    }

    private IEnumerator GameRoutine(TGBaseScene scene)
    {
        scene.Init();
        scene.OnStart();

        while (scene.isActive)
        {
            scene.OnUpdate();
            m_controller.inputSetting.OnUpdate();
            yield return 1;
        }
    }

    private IEnumerator UnloadScene(TGBaseScene _scene)
    {

        extraData = _scene.additionDataToSave;
        yield return SceneManager.UnloadSceneAsync(SceneName);
    }
}