using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System;

public class TGController : MonoBehaviour
{
    public static TGController Instance;
    public DateTime startTime;
    public DateTime endTime;
    public Camera systemCam;

    public string gameNameCn;
    public TGGameConfig gameConfig;
    public TGInputSetting inputSetting;
    public TGMainGame mainGame;
    public TGResultMng resultMng;
    public TGDXCentre dxCentre;

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
        dxCentre.OnInit(this);
    }

    private void Start()
    {
        StartCoroutine(ProcessRoutine());
    }

    public void Quit()
    {
        Debug.Log("Application Quit");
        gameConfig.Close(); 
        StopAllCoroutines();
        Application.Quit();
    }

    public void ErrorQuit(string _error)
    {
        // Write down error
        Debug.LogWarning(_error);
        StopAllCoroutines();
        // Quit();
        
        EnableDiagnosis();
    }

    public void EnableDiagnosis()
    {
        dxCentre.gameObject.SetActive(true);
        dxCentre.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            dxCentre.SetActive(!dxCentre.isActive);
        }

        if (dxCentre.isActive)
            dxCentre.OnUpdate();
    }

    IEnumerator ProcessRoutine()
    {
        startTime = DateTime.Now;
        yield return StartCoroutine(gameConfig.StartRoutine(this));
        yield return StartCoroutine(inputSetting.StartRoutine(this));
        yield return StartCoroutine(mainGame.StartRoutine(this));
        endTime = DateTime.Now;
        yield return StartCoroutine(resultMng.StartRoutine(this));
        Debug.Log("Game Finsihed");
        Quit();
        yield return 1;
    }
}
