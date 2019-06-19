using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGBaseScene: MonoBehaviour {

    protected float m_startTime;
    protected float m_timePassed;

    public string SceneName {
        get { return gameObject.scene.name; }
    }

    public TGController controller { get; private set; }

    public System.Action<string> onCaptureScreen;

    public float TimePassed {

        get { return m_timePassed; }
        set { OnTimePassed( value ); }

    }

    public bool IsActive { get; protected set; }

    void Awake() {
        #if UNITY_EDITOR
        if( TGController.Instance == null ) {
            TGController.Init();
        }
        #endif
    }

    public virtual void Init() {
        controller = TGController.Instance;
    }

    public virtual void OnStart() {
        IsActive = true;

        m_startTime = Time.time;

        TimePassed = 0;

        Recalibration();
    }


    public virtual void OnUpdate() {
        if( Input.GetKeyDown( KeyCode.Escape ) ) {
            OnPressExit();
            return;
        }

        TimePassed += Time.deltaTime;
    }

    public virtual void ExitScene() {
        IsActive = false;
    }

    // 强制退出
    public virtual void ForceClose() { }


    public void DelayCall( System.Action _func, float _delay ) {
        StartCoroutine( DelayCallRoutine( _func, _delay ) );
    }

    // 校准
    public virtual void Recalibration() {
        TGInputSetting.Recalibration();
    }

    public IEnumerator CaptureScreenshot( string _name ) {

        yield return new WaitForEndOfFrame();
        // 截图
        yield return StartCoroutine( SaveMainScreenshot( _name ) );

        // 截热图
        yield return StartCoroutine( SaveHeatmapTex( _name ) );

        if( onCaptureScreen != null )

            onCaptureScreen( _name );
    }

    public virtual IEnumerator PreUnloadScene() {

        yield return 1;
    }

    // 截屏区域
    protected virtual Rect GetScreenshotCropRect() {
        return new Rect( 0, 0, Screen.width, Screen.height );
    }

    private IEnumerator SaveHeatmapTex( string _dateStr ) {
        if( controller.heatmapInput.enabled ) {

            string fileName = "heat_" + _dateStr + ".png";

            //FIXME: 要把它调整到UI底下

            controller.heatmapInput.ApplyHeatmap();
            yield return StartCoroutine( TGTextureHelper.SaveTexture( controller.heatmapInput.outputTex, fileName ) );
        }

        yield return null;
    }

    private IEnumerator SaveMainScreenshot( string _dateStr ) {

        var raw = ScreenCapture.CaptureScreenshotAsTexture();

        var rect = GetScreenshotCropRect();

        int ix = ( int )rect.x;
        int iy = ( int )rect.y;
        int iw = ( int )rect.width;

        int ih = ( int )rect.height;

        Color[] c = raw.GetPixels( ix, iy, iw, ih );

        var tex = new Texture2D( iw, ih );
        tex.SetPixels( c );
        tex.Apply( false );



        string fileName = _dateStr + ".png";
        yield return StartCoroutine( TGTextureHelper.SaveTexture( tex, fileName ) );
    }

    protected virtual void OnTimePassed( float _value ) {
        m_timePassed = _value;

    }

    protected virtual void OnPressExit() {
        if( !IsActive )
            return;

        ExitScene();
    }

    protected IEnumerator DelayCallRoutine( System.Action _func, float _delay ) {
        yield return new WaitForSeconds( _delay );

        _func();

    }

}