using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGMainGame : TGBaseBehaviour
{
    public SceneField sceneField;
    public bool isSceneFinished;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        if (!sceneField.IsLoaded)
            yield return SceneManager.LoadSceneAsync(sceneField, LoadSceneMode.Additive);

        var scn = SceneManager.GetSceneByName(sceneField);
        var baseScn = scn.GetComponent<TGBaseScene>();

        if (baseScn != null)
            baseScn.Init(_controller);

        baseScn.OnStart();

        while (baseScn.isActive)
        {
            baseScn.OnUpdate();
            yield return 1;
        }
            
        yield return SceneManager.UnloadSceneAsync(sceneField);

        Debug.Log("Main Game: Finished");
    }
}
