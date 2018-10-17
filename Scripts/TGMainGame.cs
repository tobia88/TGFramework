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
    public string sceneName;
    public SceneMatchData[] sceneMatchers;
    public bool isSceneFinished;
    public Dictionary<string, string> additionDataToSave;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        // var scene = SceneManager.GetSceneByName(sceneName);
        var scene = GetProperScene(_controller.inputSetting.DeviceType);

        if (!scene.isLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

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
            _controller.ErrorQuit("TGBaseScene no found in scene " + sceneName);
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
        yield return SceneManager.UnloadSceneAsync(sceneName);
    }

    private Scene GetProperScene(string _deviceType)
    {
        string scnName = sceneName;

        if (sceneMatchers != null)
        {
            SceneMatchData data = sceneMatchers.FirstOrDefault(s => s.deviceType == _deviceType);

            if (!string.IsNullOrEmpty(data.sceneName))
                scnName = data.sceneName;
        }

        return SceneManager.GetSceneByName(scnName);
    }
}