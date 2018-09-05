﻿using System.Collections;
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
        // _controller.fileWriter.Write(saveFileName, _controller.gameConfig.configInfo.ToString());
        string path = TGController.Instance.RootPath + saveFileName;

        INIParser ini = new INIParser();
        
        ini.Open(path);

        ini.WriteValue("ret", "名称", TGController.Instance.gameNameCn);
        ini.WriteValue("ret", "种类", "2");
        ini.WriteValue("ret", "开始时间", TGUtility.ParseDateTimeToString(TGController.Instance.startTime));
        ini.WriteValue("ret", "结束时间", TGUtility.ParseDateTimeToString(TGController.Instance.endTime));

        Dictionary<string, string> keys = TGController.Instance.mainGame.additionDataToSave;
        foreach (string k in keys.Keys)
        {
            ini.WriteValue("ret", k, keys[k]);
        }

        ini.Close();


        Debug.Log("Finished Write");
    }
}
