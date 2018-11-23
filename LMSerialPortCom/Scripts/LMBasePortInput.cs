using System;
using System.Collections;
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

    public KeyPortData GetKeyportData(string keyName)
    {
        foreach (KeyPortData k in keys)
        {
            if (k.name.FirstOrDefault(n => n == keyName) != null)
                return k;
        }

        return null;
    }
}

public class LMBasePortInput : MonoBehaviour, IPortReceiver
{
    protected bool m_isInit;
    protected byte[] m_bytes;
    protected float m_cdToReconnect;
    protected int m_byteLength;
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

        Connect();

        return true;
    }

    public void Write(string _hex)
    {
        if (Port != null)
        {
            Debug.Log("Writing to port: " + _hex);
            Port.Write(_hex);
        }
    }

    public void Connect()
    {
        Debug.Log("正在读取端口: " + portInfo.comName);
        CurrentResolver = GetProperResolver(KeyportData);
        isPortActive = SerialPortCtrl.Open(portInfo, this, true);
    }

    public void Close()
    {
        if (SerialPortCtrl != null)
            SerialPortCtrl.Close();

        if (CurrentResolver != null)
            CurrentResolver.Close();
    }

    private bool CountdownToReconnect()
    {
        return m_cdToReconnect > 20000;
    }

    private void Update()
    {
        if (!isPortActive)
            return;

        if (CountdownToReconnect())
        {
            ReconnectInFewSeconds();
        }
    }

    public void OnReceivePort(SerialPort _port)
    {
        Port = _port;
        try
        {
            if (Port.IsOpen)
            {
                if (!m_isInit)
                    InitPortResolver();

                m_byteLength = Port.BytesToRead;

                if (m_byteLength > 0)
                {
                    m_cdToReconnect = 0;
                    ReadData();
                }
                else
                {
                    m_cdToReconnect++;
                }
            }
        }
        catch (Exception _ex)
        {
            ErrorTxt = _ex.Message;
            m_cdToReconnect++;
        }
    }

    private void ReconnectInFewSeconds()
    {
        isPortActive = false;
        m_isInit = false;
        m_cdToReconnect = 0f;
        SerialPortCtrl.Close();
        StartCoroutine(ReconnectDelay());
    }

    IEnumerator ReconnectDelay()
    {
        Debug.Log("Reconnecting in 5 seconds...");
        yield return new WaitForSeconds(5);
        Debug.Log("Reconnect");
        Connect();
    }

    private void InitPortResolver()
    {

        CurrentResolver.Init(this);
        m_isInit = true;
    }

    private void ReadData()
    {
        m_cdToReconnect = 0f;
        isPortActive = true;

        m_bytes = new byte[m_byteLength];
        Port.Read(m_bytes, 0, m_byteLength);

        CurrentResolver.ResolveBytes(m_bytes);

    }

    public virtual float GetValue(int index)
    {
        var v = CurrentResolver.values;
        if (v == null)
        {
            TGController.Instance.DebugText("没有串口数据");
            return 0f;
        }

        if (index < v.Length)
        {
            return v[index].GetValue();
        }

        TGController.Instance.DebugText("取值索引" + index + "大于数组长度: " + CurrentResolver.values.Length);

        return 0f;
    }

    public virtual void Recalibration()
    {
        CurrentResolver.Recalibration();
    }

    private LMBasePortResolver GetProperResolver(KeyPortData portData)
    {
        LMBasePortResolver retval = null;

        Debug.Log("Port Data Type: " + portData.type);

        if (portData.type == "jy901")
        {
            retval = new JY901();
        }
        else if (portData.type == "m7b" || portData.type == "m7b2D")
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