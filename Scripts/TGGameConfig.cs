using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[System.Serializable]
public struct GameConfigInfo
{
    public string key;
    public string sectionName;
}

public class TGGameConfig : TGBaseBehaviour
{
    public string fileName;
    public bool finishLoaded;
    public GameConfigInfo[] configInfo;
    public Dictionary<string, string> valueDict = new Dictionary<string, string>();

    public override IEnumerator StartRoutine(TGController _controller)
    {
        while (!finishLoaded)
        {
            OnLoadConfig();
            yield return 1;
        }

        Debug.Log("Game Config: Finished\n" +  DebugConfigInfo());

        yield return 1;
    }

    private string DebugConfigInfo()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < configInfo.Length; i++)
        {
            string key = configInfo[i].key;
            sb.AppendLine(key + ": " + valueDict[key]);
        }

        return sb.ToString();
    }

    public string GetValue(string key, string defaultValue)
    {
        if (valueDict.ContainsKey(key))
            return valueDict[key];

        return defaultValue;
    }

    public int GetValue(string key, int defaultValue)
    {
        int retval = defaultValue;

        if (valueDict.ContainsKey(key))
        {
            string v = valueDict[key];

            if (!int.TryParse(v, out retval))
            {
                retval = defaultValue;
            }
        }

        return retval;
    }

    public float GetValue(string key, float defaultValue)
    {
        float retval = defaultValue;

        if (valueDict.ContainsKey(key))
        {
            string v = valueDict[key];

            if (!float.TryParse(v, out retval))
            {
                retval = defaultValue;
            }
        }

        return retval;

    }

    private void OnLoadConfig()
    {
        string path = TGController.Instance.RootPath + fileName;

        INIParser ini = new INIParser();

        ini.Open(path);

        if (string.IsNullOrEmpty(ini.iniString))
        {
            TGController.Instance.ErrorQuit("Config is NULL! Path = " + path);
            return;
        }

        Debug.Log(ini.iniString);
        for (int i = 0; i < configInfo.Length; i++)
        {
            string v = ini.ReadValue(configInfo[i].sectionName, configInfo[i].key, string.Empty);
            valueDict.Add(configInfo[i].key, v);
        }

        ini.Close();

        finishLoaded = true;
    }
}
