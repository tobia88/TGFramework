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

    public string GameNameCn { get; private set; }
    public TGSettingData settingData { get; private set; }
    public TGGameConfig gameConfig;
    public TGInputSetting inputSetting;
    public HeatmapInput heatmapInput;
    public TGMainGame mainGame;
    public TGResultMng resultMng;
    [Header("Debug")]
    public TGDXCentre dxCentre;
    public TGDXTextCentre dxTextCentre;
    public TGDXHeatmapPanel dxHeatmapPanel;

    public LMFileWriter fileWriter;
    public string RootPath
    {
        get;
        private set;
    }
    public bool IsInit { get; private set; }

    public string SceneName
    {
        get { return settingData.GetSceneNameByDeviceType(inputSetting.DeviceType); }
    }

    private void Awake()
    {
        settingData = Resources.Load<TGSettingData>("SettingData");

        if (settingData == null)
        {
            ErrorQuit("缺少Setting Data文件！务必确保Resources文件夹底下有SettingData");
            return;
        }

        Instance = this;

        gameConfig.Init(this);
        inputSetting.Init(this);
        mainGame.Init(this);
        resultMng.Init(this);

        dxCentre.OnInit(this);
        dxTextCentre.OnInit(this);
        dxHeatmapPanel.OnInit(this);

#if UNITY_EDITOR
        RootPath = Application.dataPath + "/TGFramework/";
#else
        RootPath = Application.dataPath.Replace(Application.productName + "_Data", string.Empty);
#endif
        fileWriter.Init(RootPath);

        GameNameCn = settingData.gameNameCn;

        systemCam.gameObject.SetActive(false);

        AudioMng.Init();

        IsInit = true;
    }

    private void OnApplicationQuit()
    {
        if (IsInit)
        {
            Flush();
        }
    }

    private void Start()
    {
        if (!IsInit)
            return;

        StartCoroutine(ProcessRoutine());
    }

    public void Quit()
    {
        Debug.Log("Game Finished");
        Flush();
        Application.Quit();
    }

    private void Flush()
    {
        IsInit = false;
        gameConfig.Close();
        inputSetting.Close();
        Debug.Log("Application Quit");
        StopAllCoroutines();
    }

    public void DebugText(string _txt)
    {
        dxCentre.DebugText(_txt);
    }

    public void SetHeatmapEnable(bool _enable)
    {
        heatmapInput.enabled = settingData.outputHeatmap && _enable;

        if (_enable)
        {
            heatmapInput.Init(Screen.width, Screen.height);
            dxHeatmapPanel.SetTexture(heatmapInput.outputTex);
        }
        else
        {
            dxHeatmapPanel.ShowWarning(inputSetting.DeviceName);
        }
    }

    public void WriteLine(string _line)
    {
        if (dxTextCentre.isActive)
            dxTextCentre.WriteLine(_line);
    }

    public void RenameChinese(string cnName)
    {
        GameNameCn = cnName;
    }

    public void ErrorQuit(string _error)
    {
        // Write down error
        Debug.LogWarning(_error);
        StopAllCoroutines();
        systemCam.gameObject.SetActive(true);
        DebugText(_error);

        EnableDiagnosis();
    }

    public void EnableDiagnosis()
    {
        dxCentre.gameObject.SetActive(true);
        dxCentre.SetActive(true);
    }

    private void Update()
    {
        if (!IsInit)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            return;
        }

        DebugInputUpdate();
    }

    private void DebugInputUpdate()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.BackQuote))
        {
            dxCentre.SetActive(!dxCentre.isActive);
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha1))
        {
            dxTextCentre.SetActive(!dxTextCentre.isActive);
        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Alpha2))
        {
            dxHeatmapPanel.SetActive(!dxHeatmapPanel.isActive);
        }

        if (dxCentre.isActive)
            dxCentre.OnUpdate();
    }

    IEnumerator ProcessRoutine()
    {
        startTime = DateTime.Now;

        // TODO: 读取界面

        yield return StartCoroutine(gameConfig.SetupRoutine());
        yield return StartCoroutine(inputSetting.SetupRoutine());
        yield return StartCoroutine(mainGame.SetupRoutine());
        
        // TODO: 释放读取界面
        if (mainGame.CurrentScene != null)
        {
            yield return StartCoroutine(mainGame.GameRoutine());
            yield return StartCoroutine(mainGame.UnloadScene());
        }

        endTime = DateTime.Now;

        yield return StartCoroutine(resultMng.SetupRoutine());

        Quit();
    }
}