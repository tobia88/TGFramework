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
    protected bool m_isInit;
    protected byte[] m_bytes;
    public SerialPort Port { get; private set; }
    public LMBasePortResolver CurrentResolver { get; private set; }
    public LMSerialPortCtrl SerialPortCtrl { get; private set; }

    public string ErrorTxt
    {
        get;
        protected set;
    }


    public PortInfo portInfo;
    public bool isPortActive = false;
    public KeyPortData KeyportData { get; private set; }

    public virtual bool OnStart(KeyPortData portData)
    {
        SerialPortCtrl = GetComponent<LMSerialPortCtrl>();

        KeyportData = portData;

        if (!SerialPortCtrl.CheckPortAvailable(portInfo.comName))
        {
            ErrorTxt = "端口" + portInfo.comName + "不存在！";
            return false;
        }

        CurrentResolver = GetProperResolver(portData);

        isPortActive = true;
        SerialPortCtrl.Open(portInfo, this, true);

        return true;
    }

    public void OnReceivePort(SerialPort _port)
    {
        Port = _port;
        try
        {
            if (Port.IsOpen)
            {
                if (!m_isInit)
                {
                    CurrentResolver.Init(this);
                    m_isInit = true;
                }

                int byteLength = Port.BytesToRead;

                if (byteLength <= 0)
                    return;

                m_bytes = new byte[byteLength];
                Port.Read(m_bytes, 0, byteLength);

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

        if (portData.type == "jy901") 
        {
            retval = new JY901();
        }
        else if (portData.type == "m7b")
        {
            retval = new Leadiy_M7B();
        }
        else
        {
            retval = new LMKeyResolver();
        }

        return retval;
    }
}