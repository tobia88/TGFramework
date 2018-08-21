using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public struct GameConfigInfo
{
    public string trainingPart;
    public string deviceName;
    public string evaluatedValue;
    public string trainingDevice;
    public int damping;
    public int difficultyLv;
    public int trainingTime;
    public int waitingTime;
    public string intervalMethod;
    public int currentScore;
    public string patientName;
    public string patientId;
    public string gender;
    public int maxRotateDegForward;
    public int maxRotateDegBackward;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(trainingPart);
        sb.AppendLine(deviceName);
        sb.AppendLine(evaluatedValue);
        sb.AppendLine(trainingDevice);
        sb.AppendLine(intervalMethod);
        sb.AppendLine(gender);
        sb.AppendLine(maxRotateDegForward.ToString());
        sb.AppendLine(maxRotateDegBackward.ToString());
        return sb.ToString();
    }
}

public class TGGameConfig : TGBaseBehaviour
{
    public string fileName;
    public bool finishLoaded;
    public GameConfigInfo configInfo;

    public override IEnumerator StartRoutine(TGController _controller)
    {
        while (!finishLoaded)
        {
            OnLoadConfig();
            yield return 1;
        }

        Debug.Log("Game Config: Finished = " +  configInfo.ToString());

        yield return 1;
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

        configInfo.trainingPart         = ini.ReadValue("PZConf", "训练部位", string.Empty);
        configInfo.deviceName           = ini.ReadValue("PZConf", "设备名称", string.Empty);
        configInfo.evaluatedValue       = ini.ReadValue("PZConf", "体侧", string.Empty);
        configInfo.trainingDevice       = ini.ReadValue("PZConf", "训练器材", string.Empty);
        configInfo.damping              = ini.ReadValue("PZConf", "阻尼设置", 0);
        configInfo.difficultyLv         = ini.ReadValue("PZConf", "难度等级", 0);
        configInfo.trainingTime         = ini.ReadValue("PZConf", "训练时长", 0);
        configInfo.waitingTime          = ini.ReadValue("PZConf", "等待时长", 0);
        configInfo.intervalMethod       = ini.ReadValue("PZConf", "间隔时长", string.Empty);
        configInfo.patientName          = ini.ReadValue("PZConf", "姓名", string.Empty);
        configInfo.gender               = ini.ReadValue("PZConf", "性别", string.Empty);
        configInfo.maxRotateDegForward  = ini.ReadValue("PZConf", "旋前最大距离", -1);
        configInfo.maxRotateDegBackward = ini.ReadValue("PZConf", "旋后最大距离", -1);

        Debug.Log(configInfo.trainingTime);

        ini.Close();

        finishLoaded = true;
    }

    private void OnFinishRead(List<string> obj)
    {
        try
        {
            Debug.Log(obj.ToListString());

            configInfo.trainingPart = obj[0];
            configInfo.deviceName = obj[1];
            configInfo.evaluatedValue = obj[2];
            configInfo.trainingDevice = obj[3];
            configInfo.damping = int.Parse(obj[4]);
            configInfo.difficultyLv = int.Parse(obj[5]);
            configInfo.trainingTime = int.Parse(obj[6]);
            configInfo.waitingTime = int.Parse(obj[7]);
            configInfo.intervalMethod = obj[8];
        }
        catch (Exception _e)
        {
            TGController.Instance.ErrorQuit(_e.ToString());
        }

        finishLoaded = true;
    }
}
