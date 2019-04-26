// #define USE_TOUCH_IF_DISCONNECT

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class TGInputSetting : TGBaseBehaviour
{
    public string configFileName;
    public LMBasePortInput portInput { get; private set; }
    public LMTouchCtrl touchCtrl { get; private set; }
    public KeyInputConfig keyInputConfig;
    public bool IsPortActive
    {
        get { return (portInput == null) ? false : portInput.IsConnected; }
    }

    public bool IsTesting { get; private set; }

    public string DeviceName { get; private set; }
    public string DeviceType { get { return (KeyportData == null) ? string.Empty : KeyportData.type; } }
    public KeyPortData KeyportData { get; private set; }

    public void Close()
    {
        if (portInput != null)
            portInput.Close();
    }

    public override IEnumerator StartRoutine()
    {

        touchCtrl = GetComponent<LMTouchCtrl>();

        keyInputConfig = TGUtility.ParseConfigFile(configFileName);

        DeviceName = m_controller.gameConfig.GetValue("训练器材", string.Empty);

        Debug.Log("Current Device Name: " + DeviceName);

        KeyportData = keyInputConfig.GetKeyportData(DeviceName);

        if (KeyportData == null)
        {
            m_controller.dxErrorPopup.PopupMessage("训练器材 " + DeviceName + "不存在！");
            yield break;
        }

        touchCtrl.enabled = KeyportData.type == "touch";

        if (!touchCtrl.enabled)
        {
            yield return StartCoroutine(ConnectDeviceRoutine());
        }

        m_controller.dxErrorPopup.SetActive(false);
        m_controller.SetHeatmapEnable(KeyportData.heatmap);

        m_controller.ProgressValue += 0.1f;

        Debug.Log("Input Setup Success");
    }

    IEnumerator ConnectDeviceRoutine()
    {
        IsTesting = m_controller.gameConfig.GetValue("测试", 0) == 1;

        // FIXME: Temperory
        if (KeyportData.type == "m7b" && m_gameConfig.evalData.isFullAxis)
        {
            KeyportData.type += "2D";
        }

        portInput = GetProperInput();

        if (IsTesting)
            yield break;

        while (!portInput.IsPortActive)
        {
            yield return StartCoroutine(portInput.OnStart());

            if (!portInput.IsPortActive)
            {
                // m_controller.DebugText(portInput.ErrorTxt);
                int result = -1;

                m_controller.dxErrorPopup.PopupWithBtns(portInput.ErrorTxt, i => result = i);

                yield return new WaitUntil(() => result >= 0);

                if (result == 0)
                {
                    m_controller.Quit();
                    yield break;
                }

                m_controller.dxErrorPopup.PopupMessage("重连中");

                yield return new WaitForSecondsRealtime(0.1f);
            }
        }
    }

    public override IEnumerator EndRoutine() { yield return 1; }

    public override void ForceClose() 
    { 
        if (portInput != null)
            portInput.Close();
    }

    public void OnGameStart() 
    {
        if (portInput != null)
            portInput.SetTest(IsTesting);
    }

    private LMBasePortInput GetProperInput()
    {
        //FIXME: Temperory
        if (KeyportData.type == "CASMB")
        {
            var retval = new LMGrindTable();
            retval.Init(m_controller, 
                        KeyportData,
                        m_controller.gameConfig.GetValue("端口", -1));
            return retval;
        }

        int udp = m_controller.gameConfig.GetValue("UDP", -1);

        if (udp >= 0)
        {
            var retval = new LMInput_UDP();
            retval.Init(m_controller, KeyportData, udp);
            Debug.Log("Getting UDP On");
            return retval;
        }
        else
        {
            var retval = new LMInput_Port();
            retval.Init(m_controller,
                        KeyportData,
                        m_controller.gameConfig.GetValue("端口", -1));
            Debug.Log("Getting Por On");
            return retval;
        }
    } 
    
    public void SetPressureLevel(int level)
    {
        float[] arr = KeyportData.levels;

        if (arr == null || arr.Length == 0)
        {
            Debug.LogWarning("压力等级尚未设置");
            return;
        }

        float pressure = arr[level - 1];
        Debug.Log("压力比例设置为: " + pressure);

        if (portInput.CurrentResolver != null)
        {
            portInput.CurrentResolver.SetPressureRatio(pressure);
        }
    }

    public Vector3 GetValueFromEvalAxis()
    {
        if (!IsPortActive)
            return Vector3.zero;

        var data = m_gameConfig.evalData;
        var valueAxis = data.valueAxis;

        Vector3 values = GetValues();
        Vector3 retval = values.Reorder(valueAxis.ToString());

        if (!data.isFullAxis)
        {
            retval.y = retval.z = 0f;
        }

        return retval;
    }

    public void OnUpdate()
    {
        if (touchCtrl != null)
            touchCtrl.OnUpdate();

        if (portInput != null)
            portInput.OnUpdate();
    }

    public Vector3 GetValues()
    {
        Vector3 retval = Vector3.zero;
        if (IsPortActive)
        {
            retval.x = portInput.GetValue(0);
            retval.y = portInput.GetValue(1);
            retval.z = portInput.GetValue(2);
        }

        return retval;
    }

    public Vector3 GetRawValues()
    {
        Vector3 retval = Vector3.zero;
        if (IsPortActive)
        {
            retval.x = portInput.GetRawValue(0);
            retval.y = portInput.GetRawValue(1);
            retval.z = portInput.GetRawValue(2);
        }

        return retval;
    }

    public void Recalibration()
    {
        if (IsPortActive)
            portInput.Recalibration();
    }
}