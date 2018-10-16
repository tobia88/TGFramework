using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using UnityEngine;
using System.Linq;
using System.IO;

public class LMKeyResolver : LMBasePortResolver
{
    private byte[] m_bytes;
    private string m_getString;

    public override void ResolveBytes(byte[] _bytes)
    {
        if (_bytes.Length == 0)
            return;

        m_bytes = LMUtility.RemoveSpacing(m_bytes);

        FilterIds();
    }


    protected void FilterIds()
    {
        try
        {
            m_getString += Encoding.UTF8.GetString(m_bytes);

            for (int i = 0; i < PortData.input.Length; i++)
            {
                KeyPortInput tmpInfo = PortData.input[i];

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

    private void Flush()
    {
        Debug.Log("Flush: " + m_getString);
        m_getString = string.Empty;
    }

    public override void Recalibration()
    {
        foreach (var v in PortData.value)
            v.Recalibration();
    }

    public override float GetValue(string key)
    {
        return PortData.GetValue(key);
    }

    // public override float GetValue(string _id, float min, float max, float remapMin, float remapMax)
    // {
    //     float v = currentPortData.GetValue(_id);
    //     return TGUtility.FloatRemap(v, remapMin, remapMax, min, max);
    // }
}
