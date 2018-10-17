public abstract class LMBasePortResolver
{
    protected byte[] m_bytes;
    public string deviceName;
    public string deviceType;
    public KeyPortData PortData { get; private set; }

    public virtual void Init(KeyPortData keyPortData)
    {
        PortData = keyPortData;

        deviceName = PortData.name;
        deviceType = PortData.type;
    }

    public virtual void ResolveBytes(byte[] _bytes)
    {
        m_bytes = _bytes;
    }

    public abstract void Recalibration();
    public abstract float GetValue(string key);
}