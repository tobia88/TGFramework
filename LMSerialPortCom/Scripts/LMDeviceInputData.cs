using UnityEngine;
using System.Linq;
using TG;

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

[System.Serializable]
public class KeyPortData
{
    public string name;
    public KeyPortValue[] value;
    public KeyPortInput[] input;
    public string thresholdEquation;

    public bool m_threshold;

    private TGExpressionParser m_solver;

    public TGExpressionParser solver
    {
        get {
            if (m_solver == null)
            {
                m_solver = new TGExpressionParser();
            }
            return m_solver;
        }
    }

    public float GetInputTotal
    {
        get { return input.Sum(i => i.GetValue());}
    }
    
    public bool Threshold
    {
        get
        {
            if (string.IsNullOrEmpty(thresholdEquation))
                return true;

            m_threshold = ResolveEquation(thresholdEquation) > 0;
            return m_threshold;
        }
    }

    public float ResolveEquation(string equation)
    {
        string resolved = equation;

        foreach (var i in input)
        {
            if (resolved.IndexOf(i.key) >= 0)
            {
                string v = i.GetValue().ToString();

                if (string.IsNullOrEmpty(v))
                    v = "0";

                resolved = resolved.Replace(i.key, v);
            }
        }
        return (float)solver.EvaluateExpression(resolved).Value;
    }

    public void SetBiases(string bias)
    {
        string[] split = bias.Split(';');

        for (int i = 0; i < split.Length; i++)
        {
            string[] resolved = split[i].Split(':');

            for (int j = 0; j < input.Length; j++)
            {
                if (input[j].key == resolved[0])
                {
                    input[j].bias += float.Parse(resolved[1]);
                    continue;
                }
            }
        }
    }

    public void Update()
    {
        foreach (var val in value)
        {
            val.Update(this);
        }
    }

    public float GetValue(string key)
    {
        KeyPortValue tmpVal = GetValueByKey(key);
        float retval = tmpVal.GetValue();

        if (!Threshold)
            return 0f;

        return retval;
    }

    public void Init()
    {
        foreach(KeyPortValue v in value)
        {
            v.Init();
        }
    }

    public KeyPortValue GetValueByKey(string key)
    {
        KeyPortValue retval = value.FirstOrDefault(v => v.key == key);

        if (retval != default(KeyPortValue))
            return retval;

        throw new System.ArgumentNullException("Invalid Key: " + key);
    }
}
