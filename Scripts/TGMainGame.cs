using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGMainGame : TGBaseBehaviour
{
    public string sceneName;
    public bool isSceneFinished;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        var scn = SceneManager.GetSceneByName(sceneName);

        SceneManager.SetActiveScene(scn);
        
        var baseScn = scn.GetComponent<TGBaseScene>();

        if (baseScn != null)
        {
            baseScn.Init(_controller);
            yield return _controller.StartCoroutine(GameRoutine(baseScn));
        }

        yield return SceneManager.UnloadSceneAsync(sceneName);

        Debug.Log("Main Game: Finished");
    }

    private IEnumerator GameRoutine(TGBaseScene _scene)
    {
            _scene.OnStart();

            while (_scene.isActive)
            {
                _scene.OnUpdate();
                yield return 1;
            }

    }
}
