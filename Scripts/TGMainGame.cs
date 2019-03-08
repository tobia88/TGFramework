using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TGMainGame : TGBaseBehaviour
{
    public string SceneName { get; private set; }
    public Dictionary<string, string> additionDataToSave;

    public override IEnumerator StartRoutine()
    {
        SceneName = m_controller.SceneName;

        Scene scene = SceneManager.GetSceneByName(SceneName);


        if (!scene.isLoaded)
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);


        Debug.Log("Load Scene: " + SceneName);

        scene = SceneManager.GetSceneByName(SceneName);
        SceneManager.SetActiveScene(scene);

        var baseScn = scene.GetComponent<TGBaseScene>();

        if (baseScn != null)
        {
            yield return m_controller.StartCoroutine(GameRoutine(baseScn));
            yield return m_controller.StartCoroutine(UnloadScene(baseScn));

            Debug.Log("Main Game: Finished");
        }
        else
        {
            m_controller.ErrorQuit("TGBaseScene doesn't found on scene " + this.SceneName);
            yield break;
        }
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

        additionDataToSave = _scene.additionDataToSave;
        yield return SceneManager.UnloadSceneAsync(SceneName);
    }
}