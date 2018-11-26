using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public struct SceneMatchData
{
    public string deviceType;
    public string sceneName;
}

[CreateAssetMenu(fileName = "Setting Data", menuName = "TG-FlyingBird/TGSettingData", order = 0)]
public class TGSettingData : ScriptableObject
{
    public string gameNameCn;

    [Header ("Scene Setup")]
    public string defaultSceneName;
	public SceneMatchData[] sceneDatas;

    public string GetSceneNameByDeviceType(string _deviceType)
    {
        SceneMatchData data = sceneDatas.FirstOrDefault(s => s.deviceType == _deviceType);

        if (!string.IsNullOrEmpty(data.sceneName))
            return data.sceneName;

        return defaultSceneName;
    }
}