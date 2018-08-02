using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;

public class TGController : MonoBehaviour
{
    public static TGController Instance;

    public TGGameConfig gameConfig;
    public TGInputSetting inputSetting;
    public TGMainGame mainGame;
    public TGResultMng resultMng;

    public LMFileWriter fileWriter;
    public string RootPath
    {
        get; private set;
    }


    public string errorTxt;

    private void Awake()
    {
        Instance = this;
        AudioMng.Init();

#if UNITY_EDITOR
        RootPath = Application.dataPath + "/TGFramework/";
#else
        RootPath = Application.dataPath.Replace(Application.productName + "_Data", string.Empty);
#endif
        fileWriter.Init(RootPath);
    }

    private void Start()
    {
        StartCoroutine(ProcessRoutine());
    }

    public void Quit()
    {
        Debug.Log("Application Quit");
        StopAllCoroutines();
        Application.Quit();
    }

    public void ErrorQuit(string _error)
    {
        // Write down error
        Debug.LogError(_error);
        Quit();
    }

    IEnumerator ProcessRoutine()
    {
        yield return StartCoroutine(gameConfig.StartRoutine(this));
        yield return StartCoroutine(inputSetting.StartRoutine(this));
        yield return StartCoroutine(mainGame.StartRoutine(this));
        yield return StartCoroutine(resultMng.StartRoutine(this));
        yield return 1;
    }
}
