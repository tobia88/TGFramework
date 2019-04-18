using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class LMInput_UDP : LMBasePortInput
{

	private Thread m_receiveThread;
	private UdpClient m_client;
	private IPEndPoint m_endPoint;
	private int m_udp;

	public void Init(TGController _controller, int _udp)
	{
		Init(_controller);
		m_udp = _udp;

		m_endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), m_udp);
		m_client = new UdpClient(m_endPoint);
	}

	public override bool OpenPort()
	{
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

		if (m_client != null)
			m_client.Close();
	}

	public override void Write(byte[] bytes)
	{
		if (m_client == null)
			return;

		controller.StartCoroutine(PortWriteRoutine(bytes));
		// m_client.Send(bytes, bytes.Length, m_endPoint);
	}

	private IEnumerator PortWriteRoutine(byte[] bytes)
	{
		m_client.Send(bytes, bytes.Length, m_endPoint);

		m_isPortWriting = true;

		yield return new WaitForSeconds(0.1f);

		m_isConnected = false;
		yield return new WaitUntil(() => m_isConnected);

		Debug.Log("Write Finished");
		m_isPortWriting = false;
	}

	private void ReceiveData()
	{
		while (true)
		{
			try
			{
				m_bytes = m_client.Receive(ref m_endPoint);
				OnHandleData(m_bytes);
			}
			catch (ArgumentNullException e)
			{
				if (TGController.Instance != null)
					TGController.Instance.DebugText(e.Message);

				m_cdTick++;
			}
		}
	}
}