using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GrindTableResolver : LMBasePortResolver
{
    public const string READY_CODE_DETECTION = "CASMB";
    public const string CLEAR_PATH = "CB01FD";
    

    public LMGrindTable grindTable
    {
        get { return m_portInput as LMGrindTable; }
    }

    private string m_getString;

    public override void ResolveBytes(byte[] _bytes)
    {
        if (_bytes == null || _bytes.Length == 0)
            return;

        _bytes = LMUtility.RemoveSpacing(_bytes);

        FilterIds(_bytes);
    }

    protected void FilterIds(byte[] _bytes)
    {
        Debug.Log(Encoding.UTF8.GetString(_bytes));

        return;

        try
        {
            m_getString += Encoding.UTF8.GetString(_bytes);

            for (int i = 0; i < inputs.Length; i++)
            {
                KeyResolveInput tmpInfo = inputs[i];

                string fullId = tmpInfo.key + ":";

                int index = m_getString.IndexOf(fullId);

                if (index >= 0 && (index + fullId.Length + tmpInfo.length) < m_getString.Length)
                {
                    string v = m_getString.Substring(index + fullId.Length, tmpInfo.length);

                    if (v.Length != tmpInfo.length)
                    {
                        Debug.Log("Value Before Flush: " + v);
                        // string getting mess, just flush it
                        // Flush();
                        break;
                    }
                    else
                    {
                        tmpInfo.SetValue(float.Parse(v));
                        m_getString = m_getString.Remove(index, fullId.Length + tmpInfo.length);
                    }
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                var resolve = ResolveEquation(values[i].equation);
                values[i].SetValue(resolve);
            }

            if (m_getString != string.Empty)
                m_getString = m_getString.Replace(";", string.Empty);
        }
        catch (Exception _ex)
        {
            Debug.LogWarning(_ex);
        }
    }

    public override void Recalibration()
    {
        foreach (var v in values)
            v.Recalibration();
    }

    public override void Close()
    {
        Write(CLEAR_PATH);
    }
}