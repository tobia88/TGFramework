using UnityEngine;
using System.IO.Ports;

public class LMWitResolver : LMBasePortResolver
{
    // private Vector3[] m_values;
    // [SerializeField]
    // private Vector3 m_lastEuler, m_outputEuler, m_defaultEuler;
    // [SerializeField]
    // private WitInputData m_inputData;
    private string m_getString;

    public int[] byteValues;
    public Vector3 euler;
    public bool isInit;

    public override void Init(KeyPortData keyPortData)
    {
        base.Init(keyPortData);
        byteValues = new int[8];
    }

    public override float GetValue(string key)
    {
        Vector3 euler = Vector3.zero;
        euler.x = (values.Length > 0) ? values[0].GetValue() : 0;
        euler.y = (values.Length > 1) ? values[1].GetValue() : 0;
        euler.z = (values.Length > 2) ? values[2].GetValue() : 0;

        Vector3 retval = euler;
        // Remap from [0, 360] to [0, 1]
        if (!PortData.isDegree)
            retval /= 360;
        
        Debug.Log(retval);

        return retval.GetPosFromAxis(key);
    }

    // public void SetDefaultValue(string key, object val)
    // {
    //     float v = ((float[])val)[2];
    //     switch(key)
    //     {
    //         case "x": m_defaultEuler.x = v; break;
    //         case "y": m_defaultEuler.y = v; break;
    //         case "z": m_defaultEuler.z = v; break;
    //     }
    // }

    public override void Recalibration()
    {
        foreach(var v in values)
            v.Recalibration();
        // m_outputEuler = Vector3.zero;
    }

    public override void ResolveBytes(byte[] _bytes)
    {
        base.ResolveBytes(_bytes);

        if (m_bytes.Length == 0)
            return;

        for (int i = 0; i < m_bytes.Length; i++)
        {
            m_getString += m_bytes[i].ToString("X").PadLeft(2,'0') + " ";
        }

        int keyIndex = m_getString.IndexOf("55 53");

        if (keyIndex != -1)
        {
            var sub = m_getString.Substring(keyIndex + 6);
            var split = sub.Split(' ');

            if (split.Length >= 8)
            {
                HexToNumbers(split);
                SetupEuler();
                m_getString = string.Empty;
            }
        }
    }

    private void SetupEuler()
    {
        for (int i = 0; i < values.Length; i++)
        {
            float v = ((byteValues[i*2+1]<<8)|byteValues[i*2])/32768f * 180f;
            values[i].SetValue(v);
        }

        // // Roll
        // euler.x = ((values[1]<<8)|values[0])/32768f * 180f;

        // // Pitch
        // euler.y = ((values[3]<<8)|values[2])/32768f * 180f;

        // // Yaw
        // euler.z = ((values[5]<<8)|values[4])/32768f * 180f;

        // if (!isInit)
        // {
        //     isInit = true;
        // }
        // else
        // {
        //     ComputeOutputEuler(m_lastEuler, euler, ref m_outputEuler);
        // }

        // m_lastEuler = euler;
    }

    private void HexToNumbers(string[] _split)
    {
        string splitHex = string.Empty;

        int length = byteValues.Length;
        for (int i = 0; i < length; i++)
        {
            if (string.IsNullOrEmpty(_split[i]))
                continue;

            byteValues[i] = System.Convert.ToInt32(_split[i], 16);

            splitHex += _split[i] + " ";
        }
    }
}