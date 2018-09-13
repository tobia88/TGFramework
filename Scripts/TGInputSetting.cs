using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class PortInputSwitchData
{
    public string key;
    public LMBasePortInput portInput;
}

[System.Serializable]
public class KeyInputConfig
{
    public KeyPortData[] keys;
    public WitInputData wit;
}

public class TGInputSetting : TGBaseBehaviour
{
    public string configFileName;
    public PortInputSwitchData[] switchDatas;
    public KeyInputConfig keyInputConfig;
    public LMBasePortInput CurrentPortInput {get; private set;}
    public bool forceUsePort;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        keyInputConfig = _controller.fileWriter.ReadJSON<KeyInputConfig>(configFileName);

        string key = string.Empty;
        key = _controller.gameConfig.GetValue("设备类型", key);
        var portData = switchDatas.FirstOrDefault(p => p.key == key);

        if (portData == null)
        {
            Debug.LogWarning("Failed to match device by key " + key);
        }
        else
        {
            CurrentPortInput = portData.portInput;
            CurrentPortInput.portInfo.comName = "COM" + _controller.gameConfig.GetValue("端口", -1);

            if (!CurrentPortInput.OnStart())
            {
                if (forceUsePort)
                {
                    _controller.ErrorQuit(CurrentPortInput.ErrorTxt);
                    yield break;
                }
                else
                {
                    Debug.LogWarning(CurrentPortInput.ErrorTxt);
                }
            }
        }
        Debug.Log("Input Setup Success");


        yield return 1;
    }

    public KeyPortData GetKeyPortFromName(string keyName)
    {
        KeyPortData retval = keyInputConfig.keys.FirstOrDefault(c => c.name == keyName);

        if (retval == null)
            throw new System.NullReferenceException("Key: " + keyName + " doesnt exist in config");
        
        return retval;
    }

    public WitInputData GetWitInputData()
    {
        return keyInputConfig.wit;
    }
}
