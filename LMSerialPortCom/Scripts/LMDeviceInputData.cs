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
    [SerializeField]
    private float m_default;
    private float m_rawLastValue;
    private float m_rawValue;
    private bool m_isInit;
    
    public void SetDefaultValue(float minRange, float maxRange, float defaultVal)
    {
        m_default = defaultVal;
    }

    public void Recalibration()
    {
        value = m_default;
    }

    public float GetValue(KeyPortData data)
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


        return value;
    }

}

[System.Serializable]
public class KeyPortInput
{
    public string key;
    public int length;
    public float bias;
    public string value;

    public void SetValue(string newVal)
    {
        value = newVal;
    }

    public string GetValue()
    {
        return bias + value;
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
    private AK.ExpressionSolver m_solver;

    public AK.ExpressionSolver solver
    {
        get {
            if (m_solver == null)
            {
                m_solver = new AK.ExpressionSolver();
            }
            return m_solver;
        }
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
                string v = i.GetValue();

                if (string.IsNullOrEmpty(v))
                    v = "0000";

                resolved = resolved.Replace(i.key, v);
            }
        }
        return (float)solver.EvaluateExpression(resolved);
    }

    public void SetBiases(string bias)
    {
        Debug.Log(bias);
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


    public float GetValue(string key)
    {
        KeyPortValue tmpVal = GetValueByKey(key);
        float retval = tmpVal.GetValue(this);

        if (!Threshold)
            return 0f;

        return retval;
    }

    public void SetDefaultValue(string key, float minRange, float maxRange, float defaultVal)
    {
        KeyPortValue tmpVal = GetValueByKey(key);
        tmpVal.SetDefaultValue(minRange, maxRange, defaultVal);
    }

    public KeyPortValue GetValueByKey(string key)
    {
        KeyPortValue retval = value.FirstOrDefault(v => v.key == key);

        if (retval != default(KeyPortValue))
            return retval;

        throw new System.ArgumentNullException("Invalid Key: " + key);
    }
}
