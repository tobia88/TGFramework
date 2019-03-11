using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ConfigData
{
    public EvaluationSetupData[] infos;
}

public class TGGameConfig : TGBaseBehaviour
{
    public string fileName;
    public string sectionName;
    public string evaluationFileName;
    public ConfigData configData;
    public EvaluationSetupData evaluationSetupData;

    private INIParser m_iniParser;

    public override IEnumerator StartRoutine()
    {
        InitParser();
        if (!string.IsNullOrEmpty(evaluationFileName))
        {
            configData = m_controller.fileWriter.ReadJSON<ConfigData>(evaluationFileName);

            if (configData != null)
            {
                string cnTitle = GetValue("体侧", string.Empty);
                evaluationSetupData = GetConfigDataFromTitle(cnTitle);
            }
        }
        else
        {
            Debug.LogWarning(evaluationFileName + "Has not found");
        }

        yield return 1;
    }
    private EvaluationSetupData GetConfigDataFromTitle(string cnTitle)
    {
        return configData.infos.FirstOrDefault(d => d.cnTitle == cnTitle);
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
        string path = m_controller.RootPath + "/" + fileName;

        m_iniParser = new INIParser();
        m_iniParser.Open(path);

        if (string.IsNullOrEmpty(m_iniParser.iniString))
        {
            m_controller.ErrorQuit("Config is NULL! Path = " + path);
            return;
        }
    }
}