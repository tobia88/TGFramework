public abstract class LMBasePortResolver
{
    protected byte[] m_bytes;
    protected LMBasePortInput m_portInput;

    public string deviceType;
    public KeyPortData PortData { get; private set; }
    public KeyResolveValue[] values;
    public KeyResolveInput[] inputs;
    public int AxisAmount
    {
        get
        {
            if (values == null)
                return 0;

            return values.Length;
        }
    }

    public virtual void Init(LMBasePortInput _portInput)
    {
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
            values[i] = new KeyResolveValue(datas[i], PortData.isDegree, PortData.raw, datas[i].reverse);
        }
    }
}