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
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);
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

    public IEnumerator TakeScreenshot(string dateSpr)
    {
        yield return m_controller.StartCoroutine(CurrentScene.RecordFrame(dateSpr));
    }

    public override IEnumerator EndRoutine()
    {
<<<<<<< HEAD
        if (CurrentScene == null)
            yield break;

=======
        yield return m_controller.StartCoroutine(CurrentScene.PreUnloadScene());
>>>>>>> 6ebe208b2664d0be7d2a10b2da09269b390668d0
        yield return SceneManager.UnloadSceneAsync(SceneName);
    }


    private TGBaseScene SetSceneActive(string sceneName)
    {
        var tmpScene = SceneManager.GetSceneByName(SceneName);
        SceneManager.SetActiveScene(tmpScene);

        return tmpScene.GetComponent<TGBaseScene>();
    }

}