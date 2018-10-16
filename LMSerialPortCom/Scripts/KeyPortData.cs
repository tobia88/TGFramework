using TG;
using System.Linq;  

[System.Serializable]
public class KeyPortData
{
    public string name;
    public string type;
    public string order;
    public string thresholdEquation;
    public KeyPortValue[] value;
    public KeyPortInput[] input;

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
