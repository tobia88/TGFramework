using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System.Text;

public class LMInput_UDP : LMBasePortInput, IPortReceiver
{

	private Thread m_receiveThread;
	private UdpClient m_client;
	private IPEndPoint m_endPoint;
	// Test
	private LMSerialPortCtrl m_serialPortCtrl;
	private int m_udp;

	public void Init(TGController _controller, int _udp)
	{
		Init(_controller);
		m_udp = _udp;
	}

	private void TestUpload()
	{
		// Test
		m_serialPortCtrl = GameObject.FindObjectOfType<LMSerialPortCtrl>();

		PortInfo portInfo = new PortInfo();
		portInfo.comName = "COM9";
		portInfo.baudRate = 115200;

		m_serialPortCtrl.Open(portInfo, this);

	}

	public void OnReceivePort(SerialPort port)
	{
		if (port.IsOpen)
		{
			int length = port.BytesToRead;
			if (length > 0)
			{
				byte[] tmpBytes = new byte[length];
				port.Read(tmpBytes, 0, length);
				m_client.Send(tmpBytes, length, m_endPoint);
			}
		}
	}

	public override bool OnStart(KeyPortData portData)
	{
		base.OnStart(portData);

		try
		{
			m_endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_udp);
			m_client = new UdpClient(m_endPoint);
		}
		catch (SocketException e)
		{
			ErrorTxt = e.Message;
			return false;
		}

		TGController.Instance.DebugText("正在读取UDP");
		m_receiveThread = new Thread(new ThreadStart(ReceiveData));
		m_receiveThread.IsBackground = true;
		m_receiveThread.Start();
		return true;
	}

	public override void Close()
	{
		base.Close();

		if (m_receiveThread != null && m_receiveThread.IsAlive)
			m_receiveThread.Abort();

		if (m_serialPortCtrl != null)
			m_serialPortCtrl.Close();

		if (m_client != null)
			m_client.Close();
	}

	public override void Write(byte[] bytes)
	{
		if (m_client == null)
			return;

		m_client.Send(bytes, bytes.Length, m_endPoint);
	}

	private void ReceiveData()
	{
		while (true)
		{
			try
			{
				m_bytes = m_client.Receive(ref m_endPoint);
				ReadData(m_bytes);
			}
			catch (ArgumentNullException e)
			{
				if (TGController.Instance != null)
					TGController.Instance.DebugText(e.Message);

				m_cdToReconnect++;
			}
		}
	}
}