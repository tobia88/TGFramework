using UnityEngine;
using System.IO.Ports;



public class LMWitMotionCtrl : LMBasePortInput
{
    private Vector3[] m_values;
    [SerializeField]
    private Vector3 m_lastEuler, m_outputEuler, m_defaultEuler;
    [SerializeField]
    private WitInputData m_inputData;

    public int[] values;
    public Vector3 euler;
    public bool isInit;

    public override bool OnStart()
    {
        if (base.OnStart())
        {
            m_inputData = controller.inputSetting.GetWitInputData();
            return m_inputData != null;
        }

        return false;
    }
    
    public override float GetValue(string key, float min, float max, float remapMin, float remapMax)
    {
        Vector3 retval = m_outputEuler;
        
        if (m_inputData != null)
            retval = m_inputData.GetValue(retval);

        if (key == "x")
            return TGUtility.FloatRemap(retval.x, remapMin, remapMax, min, max);

        else if (key == "y")
            return TGUtility.FloatRemap(retval.y, remapMin, remapMax, min, max);

        else if (key == "z")
            return TGUtility.FloatRemap(retval.z, remapMin, remapMax, min, max);

        throw new System.ArgumentException("Key is only accept x, y or z");
    }

    public override float GetValue(string key)
    {
        Vector3 retval = m_outputEuler;
        
        if (m_inputData != null)
            retval = m_inputData.GetValue(retval);

        if (key == "x")
            return retval.x;

        else if (key == "y")
            return retval.y;
            
        else if (key == "z")
            return retval.z;

        throw new System.ArgumentException("Key is only accept x, y or z");

    }

    public override void SetDefaultValue(string key, object val)
    {
        float v = ((float[])val)[2];
        switch(key)
        {
            case "x": m_defaultEuler.x = v; break;
            case "y": m_defaultEuler.y = v; break;
            case "z": m_defaultEuler.z = v; break;
        }
    }

    public override void Recalibration()
    {
        m_outputEuler = m_defaultEuler;
    }

    protected override void ReceiveBytes(byte[] _bytes)
    {
        if (_bytes.Length == 0)
            return;

        for (int i = 0; i < m_bytes.Length; i++)
        {
            m_getString += _bytes[i].ToString("X").PadLeft(2,'0') + " ";
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
        
        // Roll
        euler.x = ((values[1]<<8)|values[0])/32768f * 180f;

        // Pitch
        euler.y = ((values[3]<<8)|values[2])/32768f * 180f;

        // Yaw
        euler.z = ((values[5]<<8)|values[4])/32768f * 180f;

        if (!isInit)
        {
            isInit = true;
        }
        else
        {
            ComputeOutputEuler(m_lastEuler, euler, ref m_outputEuler);
        }
        m_lastEuler = euler;
    }

    private void ComputeOutputEuler(Vector3 _lastEuler, Vector3 _newEuler, ref Vector3 _output)
    {
        _output.x = TGUtility.PreventValueSkipping(_output.x, _lastEuler.x, _newEuler.x);
        _output.y = TGUtility.PreventValueSkipping(_output.y, _lastEuler.y, _newEuler.y);
        _output.z = TGUtility.PreventValueSkipping(_output.z, _lastEuler.z, _newEuler.z);
    }

    private void HexToNumbers(string[] _split)
    {
        string splitHex = string.Empty;

        int length = values.Length;
        for (int i = 0; i < length; i++)
        {
            if (string.IsNullOrEmpty(_split[i]))
                continue;

            // Debug.Log("Index : " + i + ", Value: " + _split[i]);
            values[i] = System.Convert.ToInt32(_split[i], 16);

            splitHex += _split[i] + " ";
        }

        // Debug.Log("Result: " + splitHex);
    }
}