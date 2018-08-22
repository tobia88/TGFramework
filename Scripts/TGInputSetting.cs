using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGInputSetting : TGBaseBehaviour
{
    public LMBasePortInput portInput;
    public bool forceUsePort;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        #if !UNITY_EDITOR
        // forceUsePort = true;
        portInput.portInfo.comName = "COM3";
        #endif

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
