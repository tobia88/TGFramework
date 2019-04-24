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

	public override IEnumerator OnStart(KeyPortData portData, LMBasePortResolver resolver = null)
	{
		KeyportData = portData;

		if (!OpenPort())
			yield break;

		Debug.Log("Port Active, receiving data");

		CurrentResolver = (resolver != null) ? resolver : GetProperResolver(KeyportData);

		if (CurrentResolver != null)
		{
			CurrentResolver.Init(this);
			yield return controller.StartCoroutine(OnStartResolver());
		}

		IsPortActive = m_isConnected = true;

		if (!IsConnected)
		{
			Close();
			ErrorTxt = "连接失败，请检查设备是否正确连接";
		}
	}

	protected override IEnumerator OnStartResolver()
	{
		CurrentResolver.Start();

		yield return controller.StartCoroutine(TestConnect());
	}

	public override bool OnUpdate() { return true; }

	public override bool OpenPort()
	{
		TGController.Instance.DebugText("正在读取UDP");

		m_receiveThread = new Thread(new ThreadStart(ReceiveData));
		m_receiveThread.IsBackground = true;
		m_receiveThread.Start();

		return true;
	}

	public override IEnumerator TestConnect()
	{
		IsPortActive = true;
		m_isConnected = true;
		yield return 1;
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

		m_client.Send(bytes, bytes.Length, m_endPoint);
	}

	private void ReceiveData()
	{
		while (true)
		{
			try
			{
				Bytes = m_client.Receive(ref m_endPoint);
				OnHandleData(Bytes);
			}
			catch (ArgumentNullException e)
			{
				if (TGController.Instance != null)
					TGController.Instance.DebugText(e.Message);
			}
		}
	}
}