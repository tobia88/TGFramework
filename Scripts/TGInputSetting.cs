using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGInputSetting : TGBaseBehaviour
{
    public LMBasePortInput portInput;
    public bool forceUsePort;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        portInput = GetComponentInChildren<LMBasePortInput>();
        portInput.portInfo.comName = "COM" + _controller.gameConfig.configInfo.com;

        if (!portInput.OnStart())
        {
            if (forceUsePort)
            {
                _controller.ErrorQuit(portInput.ErrorTxt);
                yield break;
            }
            else
            {
                Debug.LogWarning(portInput.ErrorTxt);
            }
        }
        Debug.Log("Input Setup Success");


        yield return 1;
    }
}
