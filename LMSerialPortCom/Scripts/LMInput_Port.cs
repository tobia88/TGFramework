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

	public override bool OnStart(KeyPortData portData)
	{
		base.OnStart(portData);

		if (!SerialPortCtrl.CheckPortAvailable(portInfo.comName))
		{
			ErrorTxt = "端口" + portInfo.comName + "不存在！";
			return false;
		}

		ConnectPort();

		return true;
	}

	public override void Write(byte[] bytes)
	{
		if (Port != null && Port.IsOpen)
		{
			m_controller.StartCoroutine(PortWriteRoutine(bytes));
		}
	}

	private IEnumerator PortWriteRoutine(byte[] bytes)
	{
		Port.Write(bytes, 0, bytes.Length);
		IsPortWriting = true;

		yield return new WaitForSeconds(1f);
		yield return new WaitUntil(() => m_bytes != null && m_bytes.Length > 0);

		IsPortWriting = false;
	}

	public void ConnectPort()
	{
		TGController.Instance.DebugText("正在读取端口: " + portInfo.comName);
		isPortActive = SerialPortCtrl.Open(portInfo, this, true);
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
		m_controller.StartCoroutine(ReconnectDelay());
	}

	IEnumerator ReconnectDelay()
	{
		TGController.Instance.DebugText("正在重新链接串口...");
		yield return new WaitForSeconds(5);
		TGController.Instance.DebugText("连接串口中...");
		ConnectPort();
	}
}