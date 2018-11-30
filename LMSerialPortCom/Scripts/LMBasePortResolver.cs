using TG;

public abstract class LMBasePortResolver
{
    protected byte[] m_bytes;
    protected LMBasePortInput m_portInput;
    protected TGExpressionParser m_solver;

    public string deviceType;
    public KeyPortData PortData { get; private set; }
    public KeyResolveValue[] values;
    public KeyResolveInput[] inputs;

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

        PortData = m_portInput.KeyportData;

        deviceType = PortData.type;

        InitInputs(PortData.input);
        InitValues(PortData.value);
    }

    public virtual void ResolveBytes(byte[] _bytes)
    {
        m_bytes = _bytes;
    }

    public abstract void Recalibration();
    // public abstract float GetValue(string key);

    public virtual void Close() {}

    public virtual float GetValue(int index)
    {
        if (values == null)
        {
            TGController.Instance.DebugText("没有串口数据");
            return 0f;
        }

        if (index < values.Length)
        {
            return values[index].GetValue();
        }

        TGController.Instance.DebugText("取值索引" + index + "大于数组长度: " + values.Length);

        return 0f;
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
                                            PortData.raw, datas[i].reverse);
        }
    }
}