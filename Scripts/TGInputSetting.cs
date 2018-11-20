using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TGInputSetting : TGBaseBehaviour
{
    public string configFileName;
    public LMBasePortInput portInput { get; private set; }
    public bool forceUsePort;
    public LMTouchCtrl touchCtrl { get; private set; }
    public KeyInputConfig keyInputConfig;
    public bool IsPortActive
    {
        get { return portInput.isPortActive; }
    }

    public string DeviceName { get; private set; }

    public string DeviceType
    {
        get
        {
            if (portInput.CurrentResolver != null)
                return portInput.CurrentResolver.deviceType;

            return "touch";
        }
    }

    public int AxisAmount
    {
        get
        {
            if (portInput.CurrentResolver != null)
                return portInput.CurrentResolver.AxisAmount;

            return -1;
        }
    }

    public override IEnumerator StartRoutine(TGController _controller)
    {
        portInput = GetComponent<LMBasePortInput>();
        touchCtrl = GetComponent<LMTouchCtrl>();

        keyInputConfig = TGUtility.ParseConfigFile(configFileName);

        DeviceName = _controller.gameConfig.GetValue("训练器材", string.Empty);

        KeyPortData portData = keyInputConfig.GetKeyportData(DeviceName);

        if (portData == null)
        {
            _controller.ErrorQuit("训练器材 " + DeviceName + "不存在！");
            yield break;
        }

        touchCtrl.enabled = portData.type == "touch";

        if (!touchCtrl.enabled)
        {
            portInput.portInfo.comName = "COM" + _controller.gameConfig.GetValue("端口", -1);

            if (!portInput.OnStart(portData))
            {
                // Debug.LogWarning(portInput.ErrorTxt);
                _controller.DebugText(portInput.ErrorTxt);
                touchCtrl.enabled = true;
            }
        }

        Debug.Log("Input Setup Success");

        yield return 1;
    }

    public float GetValueFromEvalAxis()
    {
        return GetValue((int)TGController.Instance.evaluationSetupData.valueAxis);
    }

    public float GetValue(int index)
    {
        if (IsPortActive)
            return portInput.GetValue(index);

        return 0f;
    }

    public Vector3 GetValues(string order)
    {
        Vector3 retval = Vector3.zero;
        if (IsPortActive)
        {
            retval.x = portInput.GetValue(0);
            retval.y = portInput.GetValue(1);
            retval.z = portInput.GetValue(2);
            retval.Reorder(order);
        }
        return retval;
    }

    public void Recalibration()
    {
        if (IsPortActive)
            portInput.Recalibration();
    }
}