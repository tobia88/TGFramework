using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGBaseScene: MonoBehaviour {

    protected float m_startTime;
    protected float m_timePassed;

    public TGController controller { get; private set; }

    public System.Action<string> onCaptureScreen;

    public float TimePassed {
        get { return m_timePassed; }
        set { OnTimePassed( value ); }
    }

    public bool isActive = false;

    public virtual void Init() {
        controller = TGController.Instance;
    }

    public virtual void OnStart() {
        isActive = true;

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
        isActive = false;
    }

    // 强制退出
    public virtual void ForceClose() {}

    public void DelayCall( System.Action _func, float _delay ) {
        StartCoroutine( DelayCallRoutine( _func, _delay ) );
    }

    // 校准
    public virtual void Recalibration() {
        controller.inputSetting.Recalibration();
    }

    public IEnumerator CaptureScreenshot() {
        var dateStr = TGData.endTime.ToFileFormatString();
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine( SaveMainScreenshot( dateStr ) );
        yield return StartCoroutine( SaveHeatmapTex( dateStr ) );
        if( onCaptureScreen != null )
            onCaptureScreen( dateStr );
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
            TGData.SaveScreenshot( fileName );
        }

        yield return null;
    }

    private IEnumerator SaveMainScreenshot( string _dateStr ) {
        var raw = ScreenCapture.CaptureScreenshotAsTexture();

        var rect = GetScreenshotCropRect();

        int ix = ( int ) rect.x;
        int iy = ( int ) rect.y;
        int iw = ( int ) rect.width;
        int ih = ( int ) rect.height;

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
        if( !isActive )
            return;

        ExitScene();
    }

    IEnumerator DelayCallRoutine( System.Action _func, float _delay ) {
        yield return new WaitForSeconds( _delay );
        _func();
    }
}