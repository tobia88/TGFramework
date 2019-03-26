﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO.Ports;

public class LMInput_UDP_Server : IPortReceiver
{
    private Thread m_sendThread;
    private UdpClient m_client;
    private IPEndPoint m_endPoint;
    private int m_udp;
    private LMSerialPortCtrl m_serialPortCtrl;
    private byte[] m_bytes;

    public System.Action<byte[]> onSendBytes;

    public void Init()
    {
        m_serialPortCtrl = GameObject.FindObjectOfType<LMSerialPortCtrl>();
    }

    public void Close()
    {
        if (m_sendThread != null && m_sendThread.IsAlive)
            m_sendThread.Abort();

        if (m_serialPortCtrl != null)
            m_serialPortCtrl.Close();
    }

	public void Open(int udp, int port)
	{
        m_endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), udp);
        m_client = new UdpClient(m_endPoint);

		PortInfo portInfo = new PortInfo();
		portInfo.comName = "COM" + port;
		portInfo.baudRate = 115200;

		m_serialPortCtrl.Open(portInfo, this);

        m_sendThread = new Thread(new ThreadStart(SendData));
        m_sendThread.IsBackground = true;
        m_sendThread.Start();
	}

	public void OnReceivePort(SerialPort port)
	{
		if (port.IsOpen)
		{
			int length = port.BytesToRead;
			if (length > 0)
			{
				m_bytes = new byte[length];
				port.Read(m_bytes, 0, length);
				// m_client.Send(tmpBytes, length, m_endPoint);
			}
		}
	}

    private void SendData()
    {
        while(true)
        {
            if (m_bytes != null && m_bytes.Length > 0)
            {
                m_client.Send(m_bytes, m_bytes.Length, m_endPoint);
                if (onSendBytes != null)
                    onSendBytes(m_bytes);
            }
        }
    }
}