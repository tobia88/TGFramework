using System.Collections;
using TG;

public abstract class LMBasePortResolver
{
    // protected byte[] m_bytes;
    protected LMBasePortInput m_portInput;
    protected TGExpressionParser m_solver;

    public string deviceType { get { return PortData.type; } }
    public KeyPortData PortData { get { return m_portInput.KeyportData; } }
    public KeyResolveValue[] values;
    public KeyResolveInput[] inputs;

    public void Write(string hex)
    {
        UnityEngine.Debug.Log("Write Hex: " + hex);
        if (m_portInput != null)
            m_portInput.Write(hex);
    }

    public float ResolveEquation(string equation)
    {
        string resolved = equation;

        foreach (var i in inputs)
        {
            if (resolved.IndexOf(i.key) >= 0)
            {
                string v = i.GetValue().ToString();

                if (string.IsNullOrEmpty(v))
                    v = "0";

                resolved = resolved.Replace(i.key, v);
            }
        }

        return (float)m_solver.EvaluateExpression(resolved).Value;
    }

    public virtual void Init(LMBasePortInput _portInput)
    {
        m_solver = new TGExpressionParser();

        m_portInput = _portInput;

        InitInputs(PortData.input);
        InitValues(PortData.value);
    }

    public virtual void Start() {}

    public abstract void ResolveBytes(byte[] _bytes);

    public abstract void Recalibration();

    public virtual void Close() {}

    public void SetPressureRatio(float _ratio)
    {
        foreach (KeyResolveValue v in values)
        {
            v.SetRatio(_ratio);
        }
    }

    protected bool CheckIfPortValueExist(int index)
    {
        return values != null && index < values.Length;
    }

    public virtual float GetValue(int index)
    {
        if (!CheckIfPortValueExist(index))
            return 0f;

        return values[index].GetValue();
    }

    public virtual float GetRawValue(int index)
    {
        if (!CheckIfPortValueExist(index))
            return 0f;

        return (float)values[index].RawValue;
    }

    public virtual IEnumerator OnFinishPortWrite() {
        yield return 1;
    }

    protected virtual void InitInputs(KeyPortInputData[] datas)
    {
        if (datas == null)
            return;

        inputs = new KeyResolveInput[datas.Length];

        for (int i = 0; i < datas.Length; i++)
        {
            var newInput = new KeyResolveInput();

            newInput.key = datas[i].key;
            newInput.length = datas[i].length;

            inputs[i] = newInput;
        }
    }

    protected virtual void InitValues(KeyPortValueData[] datas)
    {
        if (datas == null)
            return;

        values = new KeyResolveValue[datas.Length];

        for (int i = 0; i < datas.Length; i++)
        {
            values[i] = new KeyResolveValue(datas[i], PortData.isDegree, 
                                            PortData.raw, datas[i].reverse, PortData.damp );
        }
    }
}