using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

public class LMInput_Port : LMBasePortInput, IPortReceiver
{
	public SerialPort Port { get; private set; }
	public LMSerialPortCtrl SerialPortCtrl { get; private set; }
	public PortInfo portInfo;

	public void Init(TGController _controller, int _com, LMSerialPortCtrl _serialPortCtrl)
	{
		base.Init(_controller);

		SerialPortCtrl = _serialPortCtrl;

		portInfo = new PortInfo();

		portInfo.comName = "COM" + _com;
		portInfo.baudRate = 115200;
	}

	public override IEnumerator OnStart(KeyPortData portData)
	{
		if (!SerialPortCtrl.CheckPortAvailable(portInfo.comName))
		{
			throw new InvalidPortNumberException(portInfo.comName);

		}
		yield return controller.StartCoroutine(base.OnStart(portData));
	}

	public override void Write(byte[] bytes)
	{
		if (Port != null && Port.IsOpen)
		{
			controller.StartCoroutine(PortWriteRoutine(bytes));
		}
	}

	private IEnumerator PortWriteRoutine(byte[] bytes)
	{
		Port.Write(bytes, 0, bytes.Length);

		m_isFreeze = true;

		yield return new WaitForSeconds(1f);
		yield return new WaitUntil(() => HasData);

		m_isFreeze = false;
	}

	public override bool ConnectPort()
	{

		controller.DebugText("正在读取端口: " + portInfo.comName);
		return SerialPortCtrl.Open(portInfo, this, true);
	}

	public override void Close()
	{
		base.Close();

		if (SerialPortCtrl != null)
			SerialPortCtrl.Close();
	}

	public void OnReceivePort(SerialPort _port)
	{
		Port = _port;

		byte[] tmpBytes = null;

		if (Port.IsOpen)
		{
			m_byteLength = Port.BytesToRead;

			if (m_byteLength > 0)
			{
				tmpBytes = new byte[m_byteLength];
				Port.Read(tmpBytes, 0, m_byteLength);
			}
		}

		ReadData(tmpBytes);
	}

	protected override void ReconnectInFewSeconds()
	{
		base.ReconnectInFewSeconds();
		SerialPortCtrl.Close();
		controller.StartCoroutine(ReconnectDelay());
	}

	IEnumerator ReconnectDelay()
	{
		bool result = false;

		while (!result)
		{
			controller.DebugText("正在重新链接串口...");
			yield return new WaitForSeconds(5);
			controller.DebugText("连接串口中...");
			result = ConnectPort();
		}

		m_isFreeze = false;
	}
}