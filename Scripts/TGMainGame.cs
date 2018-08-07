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
            baseScn.Init(_controller);

        baseScn.OnStart();

        while (baseScn.isActive)
        {
            baseScn.OnUpdate();
            yield return 1;
        }
            
        TGController.Instance.gameConfig.configInfo.currentScore = baseScn.Score;

        yield return SceneManager.UnloadSceneAsync(sceneName);

        AudioMng.Instance.StopAll();

        Debug.Log("Main Game: Finished");
    }
}
