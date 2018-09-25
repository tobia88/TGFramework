using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using UnityEngine;
using System.Linq;

public class LMAxisPortCtrl : LMBasePortInput
{
    [System.Serializable]
    public class AxisValue
    {
        public string key;
        public float minRange = -10f;
        public float maxRange = 10f;
        public float value;
        public float valueRemap;
        public float ratio;
        public float remapMin = -1f;
        public float remapMax = 1f;
        

        private float m_origin;
        public void SetValue(float _value)
        {
            value = Mathf.Clamp(_value - m_origin, minRange, maxRange);

            ratio = (value - minRange) / (maxRange - minRange);
            valueRemap = remapMin + (remapMax - remapMin) * ratio;
        }

        public void Reposition()
        {
            m_origin = value;
        }
    }

    public List<AxisValue> axisValues = new List<AxisValue>();
    private string[] m_split;
    private float m_defaultValue;

    protected override void ReceiveActivePortData(SerialPort _port)
    {
        m_getString = _port.ReadLine();
        m_getString = m_getString.Substring(1, m_getString.Length - 1);

        m_split = m_getString.Split(',');

        int length = Mathf.Min(m_split.Length, axisValues.Count);

        for (int i = 0; i < length; i++)
        {
            float outValue = 0f;

            if (float.TryParse(m_split[i], out outValue))
            {
                axisValues[i].SetValue(outValue);
            }
        }
    } 

    public override void Recalibration()
    {
        foreach (var av in axisValues)
        {
            av.Reposition();
        }
    }

    public override float GetValue(string key, float min, float max, float remapMin, float remapMax) 
    {
        foreach (var av in axisValues)
        {
            if (av.key == key)
                return av.valueRemap;
        }

        return -1f;
    }
}

