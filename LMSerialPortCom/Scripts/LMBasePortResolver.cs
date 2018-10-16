public abstract class LMBasePortResolver
{
    public KeyPortData PortData { get; private set; }

    public virtual void Init(KeyPortData keyPortData)
    {
        PortData = keyPortData;
    }

    public abstract void ResolveBytes(byte[] _bytes);
    public abstract void Recalibration();
    public abstract float GetValue(string key);
}