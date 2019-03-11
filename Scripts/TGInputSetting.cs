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
        get { return IsPortConnected && !portInput.IsPortWriting; }
    }
    
    public bool IsPortConnected
    {
        get { return (portInput == null) ? false : portInput.isPortActive; }
    }

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

        KeyportData = keyInputConfig.GetKeyportData(DeviceName);

        if (KeyportData == null)
        {
            m_controller.ErrorQuit("训练器材 " + DeviceName + "不存在！");
            yield break;
        }

        touchCtrl.enabled = KeyportData.type == "touch";

        if (!touchCtrl.enabled)
        {
            // FIXME: Temperory
            if (KeyportData.type == "m7b" && m_gameConfig.evaluationSetupData.isFullAxis)
                KeyportData.type += "2D";

            portInput = GetProperInput();

            if (!portInput.OnStart(KeyportData))
            {
                m_controller.DebugText(portInput.ErrorTxt);
                touchCtrl.enabled = true;
            }
        }

        m_controller.SetHeatmapEnable(KeyportData.heatmap);
        Debug.Log("Input Setup Success");

        yield return 1;
    }

    private LMBasePortInput GetProperInput()
    {
        int udp = m_controller.gameConfig.GetValue("UDP", -1);

        if (udp >= 0)
        {
            var retval = new LMInput_UDP();
            retval.Init(m_controller, udp);
            return retval;
        }
        else
        {
            var retval = new LMInput_Port();
            retval.Init(m_controller, 
                        m_controller.gameConfig.GetValue("端口", -1),
                        GetComponent<LMSerialPortCtrl>());
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

        var data = m_gameConfig.evaluationSetupData;
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