using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using UnityEngine;
using System.Linq;
using System.IO;

public class LMBasePortUtility : LMBasePortInput
{
    public KeyPortData[] keyPortData;
    public KeyPortData currentPortData { get; private set; }

    public override bool OnStart()
    {
        if (base.OnStart())
        {
            string keyName = TGController.Instance.gameConfig.GetValue("设备名称", string.Empty);
            Debug.Assert(!string.IsNullOrEmpty(keyName), "Key name doesn't exist " + keyName);
            currentPortData = TGController.Instance.inputSetting.GetKeyPortFromName(keyName);
            return keyPortData != null;
        }

        return false;
    }

    public override void SetDefaultValue(string key, object val)
    {
        float[] arr = (float[]) val;
        currentPortData.SetDefaultValue(key, arr[0], arr[1]);
    }

    private KeyPortData GetPortDataFromKey(string deviceName)
    {
        KeyPortData retval = keyPortData.FirstOrDefault(pd => pd.name == deviceName);

        return retval;
    }

    protected override void ReceiveBytes(byte[] _bytes)
    {
        if (_bytes.Length == 0)
            return;

        m_bytes = LMUtility.RemoveSpacing(m_bytes);

        // Print Debug Log
        m_fileWriter.Write(debugPath, m_bytes);

        FilterIds();
    }


    protected void FilterIds()
    {
        try
        {
            m_getString += Encoding.UTF8.GetString(m_bytes);

            for (int i = 0; i < currentPortData.input.Length; i++)
            {
                KeyPortInput tmpInfo = currentPortData.input[i];

                string fullId = tmpInfo.key + ":";

                int index = m_getString.IndexOf(fullId);

                if (index >= 0 && (index + fullId.Length + tmpInfo.length) < m_getString.Length)
                {
                    string v = m_getString.Substring(index + fullId.Length, tmpInfo.length);

                    if (v.Length != tmpInfo.length)
                    {
                        Debug.Log("Value Before Flush: " + v);
                        // string getting mess, just flush it
                        Flush();
                        break;
                    }
                    else
                    {
                        tmpInfo.SetValue(v);
                        m_getString = m_getString.Remove(index, fullId.Length + tmpInfo.length);
                    }
                }
            }

            if (m_getString != string.Empty)
                m_getString = m_getString.Replace(";", string.Empty);
        }
        catch (Exception _ex)
        {
            Debug.LogWarning(_ex);
        }
    }

    public void Flush()
    {
        Debug.Log("Flush: " + m_getString);
        m_getString = string.Empty;
    }

    public override void Recalibration()
    {
        foreach (var v in currentPortData.value)
            v.Recalibration();
    }

    public override float GetValue(string _id, float min, float max, float remapMin, float remapMax)
    {
        float v = currentPortData.GetValue(_id);
        return TGUtility.FloatRemap(v, remapMin, remapMax, min, max);
    }
}
