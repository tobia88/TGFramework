using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;
using UnityEngine;


[System.Serializable]
public class PortInfo
{
    public string comName = "COM4";
    public int baudRate = 9600;
    public Parity parity = Parity.None;
    public int dataBits = 8;
    public StopBits stopBits = StopBits.One;
}

public class LMSerialPortCtrl
{
    private SerialPort m_port;
    private Thread m_thread;
    private IPortReceiver m_receiver;

    public bool CheckPortAvailable(string _name)
    {
        var names = SerialPort.GetPortNames();

        foreach (var n in names)
        {
            if (n == _name)
                return true;
        }

        return false;
    }

    public bool Open(PortInfo _info, IPortReceiver _receiver, bool _isBackground = true, int _writeTimeout = 1, int _readTimeout = 1)
    {
        m_receiver = _receiver;

        m_port = new SerialPort(_info.comName, _info.baudRate, _info.parity, _info.dataBits, _info.stopBits);
        m_port.WriteTimeout = _writeTimeout;
        m_port.ReadTimeout = _readTimeout;

        try
        {
            m_port.Open();

            m_thread = new Thread(new ParameterizedThreadStart(ThreadUpdate));
            m_thread.IsBackground = _isBackground;
            m_thread.Start(m_port);

            return true;
        }
        catch (Exception _ex)
        {
            Debug.LogWarning(_ex.Message + "\tCom = " + _info.comName);
        }

        return false;
    }

    public void Close()
    {
        if (m_thread != null && m_thread.IsAlive)
        {
            m_thread.Abort();
            Debug.Log("Thread Aborted");
        }
        if (m_port != null && m_port.IsOpen)
        {
            m_port.Close();
            m_port.Dispose();
            Debug.Log("Port Closed and Disposed");
        }
    }

    private void ThreadUpdate(object _obj)
    {
        while (m_port != null && m_port.IsOpen)
        {
            if (m_receiver != null)
                m_receiver.OnReceivePort(m_port);
        }
    }
}
