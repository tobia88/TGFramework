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
    public float origin;
    public bool isDegree;
    [SerializeField]
    private float m_default;
    private float m_rawLastValue;
    private float m_rawValue;
    private bool m_isInit;
    
    public void SetDefaultValue(float minRange, float maxRange)
    {
        m_default = minRange + (maxRange - minRange) * origin;
    }

    public void Recalibration()
    {
        value = m_default;
    }

    public float GetValue(KeyPortInput[] input)
    {
        string resolved = ResolveEquation(input);

        AK.ExpressionSolver solver = new AK.ExpressionSolver();

        float newVal= (float)solver.EvaluateExpression(resolved);
        
        ConsoleProDebug.Watch("Val", newVal.ToString());

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


        return value;
    }

    private string ResolveEquation(KeyPortInput[] input)
    {
        string retval = equation;

        foreach (var i in input)
        {
            if (retval.IndexOf(i.key) >= 0)
            {
                string v = i.GetValue();

                if (string.IsNullOrEmpty(v))
                    v = "0000";

                retval = retval.Replace(i.key, v);
            }
        }

        return retval;
    }
}

[System.Serializable]
public class KeyPortInput
{
    public string key;
    public int length;
    public string value;

    public void SetValue(string newVal)
    {
        value = newVal;
    }

    public string GetValue()
    {
        return value;
    }
}

[System.Serializable]
public class KeyPortData
{
    public string name;
    public KeyPortValue[] value;
    public KeyPortInput[] input;

    public float GetValue(string key)
    {
        KeyPortValue tmpVal = GetValueByKey(key);
        return tmpVal.GetValue(input);
    }

    public void SetDefaultValue(string key, float minRange, float maxRange)
    {
        KeyPortValue tmpVal = GetValueByKey(key);
        tmpVal.SetDefaultValue(minRange, maxRange);
    }

    public KeyPortValue GetValueByKey(string key)
    {
        return value.FirstOrDefault(v => v.key == key);
    }
}
