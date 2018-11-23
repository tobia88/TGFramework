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
    public EvaluationSetupData evaluationSetupData;

    public LMFileWriter fileWriter;
    public string RootPath
    {
        get; private set;
    }

    private void Awake()
    {
        Instance = this;

        systemCam.gameObject.SetActive(false);

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
        inputSetting.Close();
        StopAllCoroutines();
        Application.Quit();
    }
    
    public void DebugText(string _txt)
    {
        dxCentre.DebugText(_txt);
    }

    public void ErrorQuit(string _error) 
    {
        // Write down error
        Debug.LogWarning(_error);
        StopAllCoroutines();
        systemCam.gameObject.SetActive(true);
        DebugText(_error); 
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
