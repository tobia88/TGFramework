﻿using System;
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
        if (Port != null && Port.IsOpen)
        {
            // Debug.Log("Write Port: " + _hex);
            Port.Write(_hex);
        }
    }

    public void Connect()
    {
        TGController.Instance.DebugText("正在读取端口: " + portInfo.comName);
        CurrentResolver = GetProperResolver(KeyportData);
        isPortActive = SerialPortCtrl.Open(portInfo, this, true);
    }

    public void Close()
    {
        if (CurrentResolver != null)
            CurrentResolver.Close();

        if (SerialPortCtrl != null)
            SerialPortCtrl.Close();
    }

    private bool CountdownToReconnect()
    {
        return m_cdToReconnect > 200000;
    }

    private void Update()
    {
        if (!isPortActive)
            return;

        if (CountdownToReconnect())
        {
            ReconnectInFewSeconds();
        }

        if (m_byteLength > 0)
        {
            TGController.Instance.DebugText("连接成功，请重新打开数据面板");
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
        TGController.Instance.DebugText("正在重新链接串口...");
        yield return new WaitForSeconds(5);
        TGController.Instance.DebugText("连接串口中...");
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
        if (CurrentResolver == null)
            return 0f;

        return CurrentResolver.GetValue(index);
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