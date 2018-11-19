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

        string equipName = _controller.gameConfig.GetValue("训练器材", string.Empty);

        KeyPortData portData = keyInputConfig.keys.FirstOrDefault(k => k.name == equipName);

        if (portData == null)
        {
            _controller.ErrorQuit("训练器材 " + equipName + "不存在！");
            yield break;
        }

        touchCtrl.enabled = portData.type == "touch";

        if (!touchCtrl.enabled)
        {
            portInput.portInfo.comName = "COM" + _controller.gameConfig.GetValue("端口", -1);

            if (!portInput.OnStart(portData))
            {
                Debug.LogWarning(portInput.ErrorTxt);
                touchCtrl.enabled = true;
            }
        }

        Debug.Log("Input Setup Success");

        yield return 1;
    }

    public float GetValueFromEvalAxis()
    {
        return GetValue(TGController.Instance.evaluationSetupData.valueAxis.ToString());
    }

    public float GetValue(string key)
    {
        if (IsPortActive)
            return portInput.GetValue(key);
        
        return 0f;
    }

    public void Recalibration()
    {
        if (IsPortActive)
            portInput.Recalibration();
    }
}