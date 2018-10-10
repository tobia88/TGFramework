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
    public string sectionName;
    // public GameConfigInfo[] configInfo;
    public Dictionary<string, string> valueDict = new Dictionary<string, string>();

    private INIParser m_iniParser;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        InitParser();
        yield return 1;
    }

    public string GetValue(string key, string defaultValue)
    {
        return m_iniParser.ReadValue(sectionName, key, defaultValue);
    }

    public int GetValue(string key, int defaultValue)
    {
        return m_iniParser.ReadValue(sectionName, key, defaultValue);
    }

    public float GetValue(string key, float defaultValue)
    {
        return (float)m_iniParser.ReadValue(sectionName, key, defaultValue);
    }

    public void Close()
    {
        m_iniParser.Close();
    }

    private void InitParser()
    {
        string path = TGController.Instance.RootPath + "/" + fileName;

        m_iniParser = new INIParser();
        m_iniParser.Open(path);

        if (string.IsNullOrEmpty(m_iniParser.iniString))
        {
            TGController.Instance.ErrorQuit("Config is NULL! Path = " + path);
            return;
        }
    }
}
