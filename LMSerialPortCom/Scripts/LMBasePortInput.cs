using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.Text;
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
	protected float m_cdTick;
	protected int m_byteLength;
	protected bool m_isConnected;

	public byte[] Bytes { get; protected set; }
	public TGController controller { get; private set; }
	public bool IsPortActive { get; protected set; }
	public virtual bool IsConnected { get { return IsPortActive && !m_isPortWriting && m_isConnected; } }
	public bool m_isPortWriting { get; protected set; }
	public LMBasePortResolver CurrentResolver { get; protected set; }
	public KeyPortData KeyportData { get; protected set; }
	public string ErrorTxt { get; protected set; }
	public bool HasData { get; protected set; }
	public float ConnectLimit { get { return 5f; } }

	public System.Action<byte[]> onReceiveDataCallback;

	private bool CountdownToReconnect()
	{
		m_cdTick += Time.deltaTime;
		return m_cdTick >= ConnectLimit;
	}

	public void Init(TGController _controller)
	{
		controller = _controller;
	}

	public virtual IEnumerator OnStart(KeyPortData portData, LMBasePortResolver resolver = null)
	{
		KeyportData = portData;

		if (!OpenPort())
			yield break;

		Debug.Log("Port Active, receiving data");

		CurrentResolver = (resolver != null) ? resolver : GetProperResolver(KeyportData);

		if (CurrentResolver != null)
			CurrentResolver.Init(this);

		yield return controller.StartCoroutine(TestConnect());

		if (CurrentResolver != null)
			yield return controller.StartCoroutine(OnStartResolver());

		if (!IsConnected)
		{
			Close();
			ErrorTxt = "连接失败，请检查设备是否正确连接";
		}
	}

	public abstract bool OpenPort();
	public abstract void Write(byte[] bytes);

	public virtual void Recalibration()
	{
		CurrentResolver.Recalibration();
	}

	public void Write(string code, bool isHex = true)
	{
		byte[] bytes = (isHex) ? TGUtility.HexToByteArray(code) : Encoding.ASCII.GetBytes(code);
		Debug.Log("Write Port: " + code + ", Converted into: " + BitConverter.ToString(bytes));
		Write(bytes);
	}

	public virtual void Close()
	{
		if (CurrentResolver != null && IsPortActive)
			CurrentResolver.Close();

		IsPortActive = false;
		Reset();
	}

	public virtual bool OnUpdate()
	{
		if (!IsPortActive || m_isPortWriting)
			return false;

		if (CountdownToReconnect())
		{
			Debug.LogWarning("Time over than limit: " + ConnectLimit + ", prepare for reconnection");;
			ReconnectInFewSeconds();
		}

		if (Bytes != null && Bytes.Length > 0)
		{
			controller.DebugText("连接成功，请重新打开数据面板");
		}

		return true;
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

	public virtual IEnumerator TestConnect()
	{
		for (int i = 0; i < 5; i++)
		{
			yield return new WaitForSeconds(1f);

			IsPortActive = m_isConnected && !m_isPortWriting;

			if (IsPortActive)
				yield break;
		}
	}

	protected virtual IEnumerator OnStartResolver()
	{
		if (!IsConnected)
			yield break;

		CurrentResolver.Start();

		yield return controller.StartCoroutine(TestConnect());
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
		else if (portData.type == "key" || portData.type == "key2D")
		{
			retval = new LMKeyResolver();
		}

		return retval;
	}

	protected virtual void OnHandleData(Byte[] bytes)
	{
		if (onReceiveDataCallback != null)
			onReceiveDataCallback(bytes);

		Bytes = bytes;

		HasData = Bytes != null && Bytes.Length > 0;

		if (!HasData)
			return;

		m_cdTick = 0f;
		m_isConnected = true;

		ResolveBytes(Bytes);
	}

	protected virtual void ResolveBytes(Byte[] bytes)
	{
		if (CurrentResolver != null)
			CurrentResolver.ResolveBytes(bytes);
	}
}