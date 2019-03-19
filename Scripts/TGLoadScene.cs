using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TGLoadScene : TGBaseBehaviour
{
    public int _currProgross = 0;
    // public Load_SceneMng load_Scene;
    public override IEnumerator StartRoutine()
    {
        yield return SceneManager.LoadSceneAsync("Load_Scene", LoadSceneMode.Additive);
        var tmpScene = SceneManager.GetSceneByName("Load_Scene");
        SceneManager.SetActiveScene(tmpScene);
        // load_Scene = tmpScene.GetComponent<Load_SceneMng>();
    }
    public override void ForceClose() {}
    public override IEnumerator EndRoutine()
    {
        yield return SceneManager.UnloadSceneAsync("Load_Scene");
    }

    //private IEnumerator UpdateRoutine()
    //{
    //    while(true)
    //    {
    //        yield return 1;
    //    }
    //}

    public void SetProgressValue(float valua)
    {
        // load_Scene.slider.value = valua;
    }
}
