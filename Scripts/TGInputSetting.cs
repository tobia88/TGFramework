using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TGInputSetting : TGBaseBehaviour
{
    public string configFileName;
    public LMBasePortInput portInput;
    public bool forceUsePort;
    public LMTouchCtrl touchCtrl;
    public KeyInputConfig keyInputConfig;
    public bool IsPortActive { get; private set; }

    public override IEnumerator StartRoutine(TGController _controller)
    {

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
            IsPortActive = portInput.OnStart(portData);

            if (!IsPortActive)
            {
                Debug.LogWarning(portInput.ErrorTxt);
                touchCtrl.enabled = true;
            }
        }

        Debug.Log("Input Setup Success");

        yield return 1;
    }

    public float GetValue(string key)
    {
        if (IsPortActive)
            return portInput.GetValue(key);

        return touchCtrl.GetValue(key);
    }

    public void Recalibration()
    {
        if (IsPortActive)
            portInput.Recalibration();
    }
}