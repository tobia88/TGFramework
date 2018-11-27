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
        get { return (portInput == null) ? false : portInput.isPortActive; }
    }

    public string DeviceName { get; private set; }

    public string DeviceType { get; private set; }

    public void Close()
    {
        portInput.Close();
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

            // FIXME: Temperory
            if (_controller.evaluationSetupData.isFullAxis)
                portData.type += "2D";

            if (!portInput.OnStart(portData))
            {
                // Debug.LogWarning(portInput.ErrorTxt);
                _controller.DebugText(portInput.ErrorTxt);
                touchCtrl.enabled = true;
            }
        }
        DeviceType = portData.type;

        Debug.Log("Input Setup Success");

        yield return 1;
    }

    public Vector3 GetValueFromEvalAxis()
    {
        if (!IsPortActive)
            return Vector3.zero;

        var data = TGController.Instance.evaluationSetupData;
        var valueAxis = data.valueAxis;

        Vector3 values = GetValues();
        Vector3 retval = values.Reorder(valueAxis.ToString());

        if (!data.isFullAxis)
        {
            retval.y = retval.z = 0f;
        }

        return retval;
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

    public void Recalibration()
    {
        if (IsPortActive)
            portInput.Recalibration();
    }
}