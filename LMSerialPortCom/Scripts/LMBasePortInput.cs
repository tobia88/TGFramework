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

public abstract class LMBasePortInput
{

	// protected bool m_isInit;
	protected byte[] m_bytes;
	protected float m_cdTick;
	protected int m_byteLength;
	protected bool m_isConnected;

	public TGController controller { get; private set; }
	public bool IsPortActive { get; protected set; }
	public bool IsConnected { get { return IsPortActive && !m_isPortWriting && m_isConnected; } }
	public bool m_isPortWriting { get; protected set; }
	public LMBasePortResolver CurrentResolver { get; protected set; }
	public KeyPortData KeyportData { get; private set; }
	public string ErrorTxt { get; protected set; }
	public bool HasData { get; protected set; }

	private bool CountdownToReconnect()
	{
		return m_cdTick > 200000;
	}

	public void Init(TGController _controller)
	{
		controller = _controller;
	}

	public abstract bool OpenPort();

	public virtual IEnumerator OnStart(KeyPortData portData)
	{
		KeyportData = portData;

		if (!OpenPort())
			yield break;

		Debug.Log("Port Active, receiving data");

		IsPortActive = true;

		CurrentResolver = GetProperResolver(KeyportData);
		CurrentResolver.Init(this);

		yield return new WaitUntil(() => IsConnected);
	}

	public virtual void Close()
	{
		if (CurrentResolver != null)
			CurrentResolver.Close();

		IsPortActive = false;
	}

	public void OnUpdate()
	{
		if (!IsPortActive || m_isPortWriting)
			return;

		if (CountdownToReconnect())
		{
			Debug.Log("重连");
			ReconnectInFewSeconds();
		}

		if (m_bytes != null && m_bytes.Length > 0)
		{
			controller.DebugText("连接成功，请重新打开数据面板");
		}
	}

	protected virtual void ReconnectInFewSeconds()
	{
		controller.DebugText("连接断开，数秒后重新连接");
		Reset();
	}

	protected void Reset()
	{
		// m_isInit = false;
		m_cdTick = 0f;
		m_isPortWriting = false;
		m_isConnected = false;
	}

	public virtual float GetValue(int index)
	{
		if (CurrentResolver == null)
			return 0f;

		return CurrentResolver.GetValue(index);
	}

	public virtual float GetRawValue(int index)
	{
		if (CurrentResolver == null)
			return 0f;

		return CurrentResolver.GetRawValue(index);
	}

	public virtual void Recalibration()
	{
		CurrentResolver.Recalibration();
	}

	public void Write(string hex)
	{
		byte[] bytes = TGUtility.StringToByteArray(hex);
		Debug.Log("Write Port: " + hex + ", Converted into: " + BitConverter.ToString(bytes));
		Write(bytes);
	}

	public abstract void Write(byte[] bytes);

	protected LMBasePortResolver GetProperResolver(KeyPortData portData)
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

	protected void ResolveData(Byte[] bytes)
	{
		m_bytes = bytes;

		HasData = m_bytes != null && m_bytes.Length > 0;

		if (!HasData)
		{
			if (m_isPortWriting)
				return;

			m_cdTick++;
		}
		else
		{
			m_cdTick = 0f;
			m_isConnected = true;

			if (CurrentResolver != null)
				CurrentResolver.ResolveBytes(m_bytes);
		}
	}
}