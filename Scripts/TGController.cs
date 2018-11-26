using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGController : MonoBehaviour
{
    public static TGController Instance;
    public DateTime startTime;
    public DateTime endTime;
    public Camera systemCam;
    public string GameNameCn
    {
        get { return settingData.gameNameCn; }
    }
    public TGSettingData settingData;
    public TGGameConfig gameConfig;
    public TGInputSetting inputSetting;
    public TGMainGame mainGame;
    public TGResultMng resultMng;
    public TGDXCentre dxCentre;
    public TGDXTextCentre dxTextCentre;
    public EvaluationSetupData evaluationSetupData;

    public LMFileWriter fileWriter;
    public string RootPath
    {
        get;
        private set;
    }

    public string SceneName
    {
        get
        {
            Debug.Log("Device Type: " + inputSetting.DeviceType + ", Result: " + (inputSetting.DeviceType == "key2D"));
            if (inputSetting.DeviceType == "key2D") return "Scene_2D";
            if (inputSetting.DeviceType == "m7b2D") return "Scene_m7b2D";
            else return "Scene_1D";
        }
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
        dxTextCentre.OnInit(this);
    }

    private void Start()
    {
        StartCoroutine(ProcessRoutine());
    }

    public void Quit()
    {
        gameConfig.Close();
        inputSetting.Close();
        Debug.Log("Application Quit");
        StopAllCoroutines();
        Application.Quit();
    }

    public void DebugText(string _txt)
    {
        dxCentre.DebugText(_txt);
    }

    public void WriteLine(string _line)
    {
        if (dxTextCentre.isActive)
            dxTextCentre.WriteLine(_line);
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

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            dxTextCentre.SetActive(!dxTextCentre.isActive);
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