using System;
using System.IO.Ports;
using System.Linq;
using UnityEngine;

public interface IPortReceiver
{
    void OnReceivePort(SerialPort port);
}

[System.Serializable]
public class KeyInputConfig
{
    public KeyPortData[] keys;
}

public class LMBasePortInput : MonoBehaviour, IPortReceiver
{
    protected LMSerialPortCtrl m_serialPortCtrl;
    protected LMFileWriter m_fileWriter;
    protected byte[] m_bytes;
    protected string m_getString;
    public LMBasePortResolver CurrentResolver { get; private set; }

    public string ErrorTxt
    {
        get;
        protected set;
    }

    public PortInfo portInfo;
    public bool isPortActive = false;

    public virtual bool OnStart(KeyPortData portData)
    {
        m_serialPortCtrl = GetComponent<LMSerialPortCtrl>();

        if (!m_serialPortCtrl.CheckPortAvailable(portInfo.comName))
        {
            ErrorTxt = "端口" + portInfo.comName + "不存在！";
            return false;
        }

        CurrentResolver = GetProperResolver(portData);

        isPortActive = true;
        m_serialPortCtrl.Open(portInfo, this, true);

        return true;
    }

    public void OnReceivePort(SerialPort _port)
    {
        try
        {
            if (_port.IsOpen)
            {
                int byteLength = _port.BytesToRead;

                if (byteLength <= 0)
                    return;

                m_bytes = new byte[byteLength];
                _port.Read(m_bytes, 0, byteLength);

                CurrentResolver.ResolveBytes(m_bytes);
            }
        }
        catch (Exception _ex)
        {
            ErrorTxt = _ex.ToString();
            Debug.LogWarning(_ex);
        }
    }

    public virtual float GetValue(string key) 
    {
        return CurrentResolver.GetValue(key);
    }

    public virtual void Recalibration()
    {
        CurrentResolver.Recalibration();
    }

    private LMBasePortResolver GetProperResolver(KeyPortData portData)
    {
        LMBasePortResolver retval = null;

        if (portData.type == "wit") 
        {
            retval = new LMWitResolver();
        }
        else
        {
            retval = new LMKeyResolver();
        }

        retval.Init(portData);
        return retval;
    }
}