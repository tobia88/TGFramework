using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGResultMng : TGBaseBehaviour
{
    public string saveFileName = "ret.txt";
    public bool isWrited = false;
    public override IEnumerator StartRoutine(TGController _controller)
    {
        ForceWrite(_controller);
        isWrited = true;
        yield return 1;
    }

    public void ForceWrite(TGController _controller)
    {
        _controller.fileWriter.Write(saveFileName, _controller.gameConfig.configInfo.ToString());
        Debug.Log("Finished Write");
    }
}
