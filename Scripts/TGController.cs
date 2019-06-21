using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGController: TGBaseBehaviour {
    public const string SCENE_NAME = "0_Controller";

    public static TGController Instance;
    public Camera systemCam;

    public TGGameConfig gameConfig;
    public TGInputSetting inputSetting;
    public HeatmapInput heatmapInput;
    public TGMainGame mainGame;
    public TGResultMng resultMng;
    public TGDebug debug;
    [Header( "Debug" )]
    public TGDXCentre dxCentre;
    public TGDXTextCentre dxTextCentre;
    public TGDXHeatmapPanel dxHeatmapPanel;
    public TGDXLoadingPanel dxLoadingPanel;

    private float m_progressValue;
    private Coroutine m_routine;

    public float ProgressValue {
        get { return m_progressValue; }
        set {
            m_progressValue = value;
            Debug.Log( "Progress: " + m_progressValue );
            OnMainGameProgressChanged( m_progressValue );
        }
    }

    public bool IsInit { get; private set; }

    private bool m_onClearEnd;

    public static void Init() {
        var prefab = Resources.Load<TGController>("TGController");
        Instance = Instantiate(prefab);
        DontDestroyOnLoad(Instance.gameObject);
        Debug.Log("创建了TGController");
    }

    private void Awake() {
        if( Instance == null )
            Instance = this;

        gameConfig.Init( this );
        inputSetting.Init( this );
        mainGame.Init( this );
        resultMng.Init( this );

        debug.Init();

        dxCentre.Init();
        dxTextCentre.Init();
        dxHeatmapPanel.Init();
        // dxErrorPopup.Init();
        dxLoadingPanel.Init();

        systemCam.gameObject.SetActive( false );

        AudioMng.Init();

        IsInit = true;
    }

    private void OnApplicationQuit() {
        if( !m_onClearEnd ) {
            ForceQuit();
        }

        Resources.UnloadUnusedAssets();
        System.GC.Collect();
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
        
        m_routine = StartCoroutine( ProcessRoutine() );
    }

    private void Update() {
        if( !IsInit ) {
            if( Input.GetKeyDown( KeyCode.Escape ) )
                Application.Quit();

            return;
        }

        DebugInputUpdate();
    }

    public void Quit() {
        Debug.Log( "游戏正常退出" );
        StopAllCoroutines();
        Application.Quit();
    }

    public void SetHeatmapEnable( bool _enable ) {
        heatmapInput.enabled = _enable;

        Debug.Log( "是否打开热图：" + heatmapInput.enabled );

        if( _enable ) {
            heatmapInput.Init( Screen.width, Screen.height );
            dxHeatmapPanel.SetTexture( heatmapInput.outputTex );
        } else {
            dxHeatmapPanel.ShowWarning( TGData.DeviceName );
        }
    }

    public void ErrorQuit( string _error ) {
        // Write down error
        TGDebug.ErrorBox( _error, 0, i => Quit(), "确定" );

        Debug.LogWarning( _error );

        StopCoroutine( m_routine );
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

    private IEnumerator ProcessRoutine() {
        yield return 1;

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