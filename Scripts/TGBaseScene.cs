using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TGBaseScene : MonoBehaviour
{
    private const string MAIN_SCREENSHOT_KEY = "图片";

    protected float m_startTime;
    protected float m_timePassed;
    public Dictionary<string, string> extraData = new Dictionary<string, string>();

    public TGController controller { get; private set; }
    public Rect screenshotCropInfo = new Rect(0, 0, 1920, 1080);

    public System.Action<string> onCaptureScreen;

    public float TimePassed
    {
        get { return m_timePassed; }
        set
        {
            OnTimePassed(value);
        }
    }

    public bool isActive = false;

    public virtual void Init()
    {
        controller = TGController.Instance;
    }

    public virtual void OnStart()
    {
        isActive = true;

        m_startTime = Time.time;

        TimePassed = 0;

        Recalibration();
    }

    public virtual void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPressExit();
            return;
        }

        TimePassed += Time.deltaTime;
    }

    protected virtual void OnPressExit()
    {
        ExitScene();
    }

    public virtual void ExitScene()
    {
        // Compensate screenshot key if it doesn't existed
        if (!extraData.ContainsKey(MAIN_SCREENSHOT_KEY))
        {
            extraData.Add(MAIN_SCREENSHOT_KEY, string.Empty);
        }

        isActive = false;
    }

    public void DelayCall(System.Action _func, float _delay)
    {
        StartCoroutine(DelayCallRoutine(_func, _delay));
    }

    public virtual void Recalibration()
    {
        controller.inputSetting.Recalibration();
    }

    public IEnumerator RecordFrame(string _dateStr)
    {
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(SaveMainScreenshot(_dateStr));
        yield return StartCoroutine(SaveHeatmapTex(_dateStr));
        if (onCaptureScreen != null)
            onCaptureScreen(_dateStr);
    }

    public virtual IEnumerator PreUnloadScene()
    {
        yield return 1;
    }

    private IEnumerator SaveHeatmapTex(string _dateStr)
    {
        if (controller.heatmapInput.enabled)
        {
            string fileName = "heat_" + _dateStr + ".png";
            controller.heatmapInput.ApplyHeatmap();
            yield return StartCoroutine(SaveTexture(controller.heatmapInput.outputTex, fileName));
            SaveScreenshotKey(MAIN_SCREENSHOT_KEY, fileName);
        }

        yield return null;
    }

    private IEnumerator SaveMainScreenshot(string _dateStr)
    {
        var raw = ScreenCapture.CaptureScreenshotAsTexture();
        Color[] c = raw.GetPixels((int)screenshotCropInfo.x,
            (int)screenshotCropInfo.y,
            (int)screenshotCropInfo.width,
            (int)screenshotCropInfo.height);
        var tex = new Texture2D((int)screenshotCropInfo.width, (int)screenshotCropInfo.height);
        tex.SetPixels(c);
        tex.Apply(false);

        string fileName = _dateStr + ".png";
        yield return StartCoroutine(SaveTexture(tex, fileName));
        SaveScreenshotKey(MAIN_SCREENSHOT_KEY, fileName);
    }

    private IEnumerator SaveTexture(Texture2D _tex, string _name)
    {
        float ratio = (float)_tex.width / _tex.height;
        int width = 0, height = 0;

        if (ratio > 1.39f)
        {
            width = 700;
            height = Mathf.RoundToInt(width / ratio);
        }
        else
        {
            height = 590;
            width = Mathf.RoundToInt(height * ratio);
        }

        _tex = TextureScaler.ResizeTexture(_tex, width, height);

        byte[] bytes = _tex.EncodeToPNG();
        GameObject.Destroy(_tex);

        Debug.Log("Write Texture: " + _name);

        controller.fileWriter.Write(_name, bytes);

        yield return null;
    }

    private void SaveScreenshotKey(string _key, string _fileName)
    {
        if (extraData.ContainsKey(_key))
        {
            extraData[_key] += "|" + _fileName;
        }
        else
        {
            extraData.Add(_key, _fileName);
        }
    }

    protected virtual void OnTimePassed(float _value)
    {
        m_timePassed = _value;
    }

    IEnumerator DelayCallRoutine(System.Action _func, float _delay)
    {
        yield return new WaitForSeconds(_delay);
        _func();
    }
}