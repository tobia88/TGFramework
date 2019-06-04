using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGController: MonoBehaviour {
    public static TGController Instance;
    public Camera systemCam;

    public string GameNameCn { get; private set; }
    public TGSettingData settingData { get; private set; }
    public TGGameConfig gameConfig;
    public TGInputSetting inputSetting;
    public HeatmapInput heatmapInput;
    public TGMainGame mainGame;
    public TGResultMng resultMng;
    [Header( "Debug" )]
    public TGDXCentre dxCentre;
    public TGDXTextCentre dxTextCentre;
    public TGDXHeatmapPanel dxHeatmapPanel;
    public TGDXErrorPopup dxErrorPopup;
    public TGDXLoadingPanel dxLoadingPanel;

    private float m_progressValue;

    public float ProgressValue {
        get { return m_progressValue; }
        set {
            m_progressValue = value;
            Debug.Log( "Progress: " + m_progressValue );
            OnMainGameProgressChanged( m_progressValue );
        }
    }

    public LMFileWriter fileWriter;
    public string RootPath {
        get;
        private set;
    }
    public bool IsInit { get; private set; }

    private bool m_onClearEnd;

    public string SceneName {
        get { return settingData.GetSceneNameByDeviceType( inputSetting.DeviceType ); }
    }

    private void Awake() {
        Instance = this;
        gameConfig.Init( this );
        inputSetting.Init( this );
        mainGame.Init( this );
        resultMng.Init( this );

        dxCentre.OnInit( this );
        dxTextCentre.OnInit( this );
        dxHeatmapPanel.OnInit( this );
        dxErrorPopup.OnInit( this );
        dxLoadingPanel.OnInit( this );

        settingData = Resources.Load<TGSettingData>( "SettingData" );

        if( settingData == null ) {
            ErrorQuit( "缺少Setting Data文件！务必确保Resources文件夹底下有SettingData" );
            return;
        }

#if UNITY_EDITOR
        RootPath = Application.dataPath + "/TGFramework/";
#else
        RootPath = Application.dataPath.Replace(Application.productName + "_Data", string.Empty);
#endif
        fileWriter.Init( RootPath );

        GameNameCn = settingData.gameNameCn;

        systemCam.gameObject.SetActive( false );

        AudioMng.Init();

        IsInit = true;
    }

    private void OnApplicationQuit() {
        if( !m_onClearEnd ) {
            ForceQuit();
        }
    }

    private void ForceQuit() {
        mainGame.ForceClose();
        gameConfig.ForceClose();
        inputSetting.ForceClose();

        StopAllCoroutines();

        Debug.Log( "游戏不正常退出" );
    }

    private void Start() {
        if( !IsInit )
            return;

        StartCoroutine( ProcessRoutine() );
    }

    public void Quit() {
        Debug.Log( "游戏正常退出" );
        StopAllCoroutines();
        Application.Quit();
    }

    public void SetHeatmapEnable( bool _enable ) {
        heatmapInput.enabled = settingData.outputHeatmap && _enable;

        Debug.Log( "是否打开热图：" + heatmapInput.enabled );

        if( _enable ) {
            heatmapInput.Init( Screen.width, Screen.height );
            dxHeatmapPanel.SetTexture( heatmapInput.outputTex );
        } else {
            dxHeatmapPanel.ShowWarning( inputSetting.DeviceName );
        }
    }

    public void RenameChinese( string cnName ) {
        GameNameCn = cnName;
    }

    public void ErrorQuit( string _error ) {
        // Write down error
        Debug.LogWarning( _error );
        StopAllCoroutines();
        systemCam.gameObject.SetActive( true );

        EnableDiagnosis();
    }

    public void EnableDiagnosis() {
        dxCentre.gameObject.SetActive( true );
        dxCentre.SetActive( true );
    }

    private void Update() {
        if( !IsInit ) {
            if( Input.GetKeyDown( KeyCode.Escape ) )
                Application.Quit();

            return;
        }

        DebugInputUpdate();
    }

    private void DebugInputUpdate() {
        if( Input.GetKey( KeyCode.LeftAlt ) && Input.GetKeyDown( KeyCode.Q ) ) {
            dxCentre.SetActive( !dxCentre.isActive );
        }

        if( Input.GetKey( KeyCode.LeftAlt ) && Input.GetKeyDown( KeyCode.W ) ) {
            dxTextCentre.SetActive( !dxTextCentre.isActive );
        }

        if( Input.GetKey( KeyCode.LeftAlt ) && Input.GetKeyDown( KeyCode.E ) ) {
            dxHeatmapPanel.SetActive( !dxHeatmapPanel.isActive );
        }

        if( dxCentre.isActive )
            dxCentre.OnUpdate();
    }

    IEnumerator ProcessRoutine() {
        dxLoadingPanel.SetActive( true );
        yield return StartCoroutine( gameConfig.StartRoutine() );
        yield return StartCoroutine( inputSetting.StartRoutine() );
        yield return StartCoroutine( mainGame.StartRoutine() );

        if( mainGame.CurrentScene != null ) {
            ProgressValue = 1f;

            yield return new WaitForSeconds( 2f );

            dxLoadingPanel.SetActive( false );

            yield return StartCoroutine( mainGame.GameRoutine() );
            yield return StartCoroutine( mainGame.EndRoutine() );
        }

        yield return StartCoroutine( resultMng.StartRoutine() );
        yield return StartCoroutine( resultMng.EndRoutine() );
        yield return StartCoroutine( inputSetting.EndRoutine() );
        yield return StartCoroutine( gameConfig.EndRoutine() );

        Quit();

        m_onClearEnd = true;
    }

    private void OnMainGameProgressChanged( float progress ) {
        dxLoadingPanel.image.fillAmount = progress;
    }
}