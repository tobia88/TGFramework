using UnityEngine;
using System.Linq;

[System.Serializable]
public class WitInputData
{
    public string order;

    public Vector3 GetValue(Vector3 output)
    {
        switch(order)
        {
            case "yzx": return new Vector3(output.y, output.z, output.x);
            case "yxz": return new Vector3(output.y, output.x, output.z);
            case "xzy": return new Vector3(output.x, output.z, output.y);
            case "zxy": return new Vector3(output.z, output.x, output.y);
            case "zyx": return new Vector3(output.z, output.y, output.x);
            default   : return output;
        }
    }
}

[System.Serializable]
public class KeyPortValue
{
    public string key;
    public string equation;
    
    public float value;
    public bool isDegree;
    public string min;
    public string max;
    public float origin;
    private float m_default;
    private float m_rawLastValue;
    private float m_rawValue;
    private bool m_isInit;
    public float Min {get; private set;}
    public float Max {get; private set;}

    public void Init()
    {
        Min = TGUtility.GetValueFromINI(min);
        Max = TGUtility.GetValueFromINI(max);

        m_default = Min + (Max - Min) * origin;
        value = m_default;
    }
    
    public void Recalibration()
    {
        value = m_default;
    }

    public void Update(KeyPortData data)
    {
        float newVal= data.ResolveEquation(equation);

        if (float.IsNaN(newVal))
            newVal = 0f;

        if (!m_isInit)
        {
            m_rawLastValue = m_rawValue = newVal;
            m_isInit = true;
        }
        else
        {
            m_rawLastValue = m_rawValue;
            m_rawValue = newVal;
        }

        if (isDegree)
        {
            value = TGUtility.PreventValueSkipping(value, m_rawLastValue, m_rawValue);
        }
        else
        {
            value += (m_rawValue - m_rawLastValue);
        }
    }

    public float GetValue()
    {
        if (Min != Max)
        {
            return (value - Min) / (Max - Min);
        }

        return value;
    }

}

[System.Serializable]
public class KeyPortInput
{
    public string key;
    public int length;
    public float bias;
    public float value;

    public void SetValue(string newVal)
    {
        value = float.Parse(newVal);
    }

    public float GetValue()
    {
        return bias + value;
    }

    public override string ToString()
    {
        return key + ": " + value.ToString();
    }
}
