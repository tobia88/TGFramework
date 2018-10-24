using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SceneMatchData
{
    public string deviceType;
    public string sceneName;
}

public class TGMainGame : TGBaseBehaviour
{
    public string defaultSceneName;
    public string SceneName { get; private set; }
    public SceneMatchData[] sceneMatchers;
    public Dictionary<string, string> additionDataToSave;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        SceneName = GetProperSceneName(_controller.inputSetting.DeviceType);

        Scene scene = SceneManager.GetSceneByName(SceneName);

        if (!scene.isLoaded)
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Additive);


        scene = SceneManager.GetSceneByName(SceneName);
        SceneManager.SetActiveScene(scene);

        var baseScn = scene.GetComponent<TGBaseScene>();

        if (baseScn != null)
        {
            yield return _controller.StartCoroutine(GameRoutine(baseScn));
            yield return _controller.StartCoroutine(UnloadScene(baseScn));

            Debug.Log("Main Game: Finished");
        }
        else
        {
            _controller.ErrorQuit("TGBaseScene no found in scene " + this.SceneName);
            yield break;
        }
    }

    private IEnumerator GameRoutine(TGBaseScene _scene)
    {
        _scene.Init();
        _scene.OnStart();

        while (_scene.isActive)
        {
            _scene.OnUpdate();
            yield return 1;
        }
    }

    private IEnumerator UnloadScene(TGBaseScene _scene)
    {

        additionDataToSave = _scene.additionDataToSave;
        yield return SceneManager.UnloadSceneAsync(SceneName);
    }

    private string GetProperSceneName(string _deviceType)
    {
        string retval = defaultSceneName;

        if (sceneMatchers != null)
        {
            Debug.Log(_deviceType);

            SceneMatchData data = sceneMatchers.FirstOrDefault(s => s.deviceType == _deviceType);

            if (!string.IsNullOrEmpty(data.sceneName))
                retval = data.sceneName;
        }

        Debug.Log("Loading Scene: " + retval);
        return retval;
    }
}