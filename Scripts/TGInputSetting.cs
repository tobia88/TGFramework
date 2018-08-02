using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGInputSetting : TGBaseBehaviour
{
    public LMBasePortUtility basePortUtility;
    public bool forceUsePort;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        #if !UNITY_EDITOR
        // forceUsePort = true;
        basePortUtility.portInfo.comName = "COM3";
        #endif

        if (!basePortUtility.OnStart())
        {
            if (forceUsePort)
            {
                _controller.ErrorQuit(basePortUtility.ErrorTxt);
                yield break;
            }
            else
            {
                Debug.LogWarning(basePortUtility.ErrorTxt);
            }
        }
        Debug.Log("Input Setup Success");


        yield return 1;
    }
}
