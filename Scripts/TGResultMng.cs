using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGResultMng : TGBaseBehaviour
{
    public string saveFileName = "ret.txt";
    public bool isWrited = false;
    public override IEnumerator SetupRoutine()
    {
        ForceWrite();
        isWrited = true;
        yield return 1;
    }

    public void ForceWrite()
    {
        string path = m_controller.RootPath + saveFileName;

        INIParser ini = new INIParser();
        
        ini.Open(path);

        ini.WriteValue("ret", "名称", m_controller.GameNameCn);
        ini.WriteValue("ret", "种类", "2");
        ini.WriteValue("ret", "开始时间", TGUtility.ParseDateTimeToString(m_controller.startTime));
        ini.WriteValue("ret", "结束时间", TGUtility.ParseDateTimeToString(m_controller.endTime));
        WriteExtraData(ini);
        ini.Close();


        Debug.Log("Finished Write");
    }

    private void WriteExtraData(INIParser ini)
    {
        Dictionary<string, string> keys = m_controller.mainGame.extraData;
        foreach (string k in keys.Keys)
        {
            ini.WriteValue("ret", k, keys[k]);
        }
    }
}
