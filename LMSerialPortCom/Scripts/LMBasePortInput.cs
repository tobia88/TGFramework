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

public class LMBasePortInput
{
	
	protected bool m_isInit;
	protected byte[] m_bytes;
	protected float m_cdToReconnect;
	protected int m_byteLength;
    protected TGController m_controller;
	public bool isPortActive = false;
	public LMBasePortResolver CurrentResolver { get; protected set; }
	public KeyPortData KeyportData { get; private set; }
	public string ErrorTxt { get; protected set; }

    public void Init(TGController _controller)
    {
        m_controller = _controller;
    }

    public virtual bool OnStart(KeyPortData portData)
    {
		KeyportData = portData;
        return true;
    }

    public virtual void Close()
    {
		if (CurrentResolver != null)
			CurrentResolver.Close();
    }

	private bool CountdownToReconnect()
	{
		return m_cdToReconnect > 200000;
	}

	public void OnUpdate()
	{
		if (!isPortActive)
			return;

		if (CountdownToReconnect())
		{
			ReconnectInFewSeconds();
		}

		if (m_bytes != null)
		{
			TGController.Instance.DebugText("连接成功，请重新打开数据面板");
		}
	}

	protected void InitPortResolver()
	{
		CurrentResolver = GetProperResolver(KeyportData);
		CurrentResolver.Init(this);
		m_isInit = true;
	}

	protected virtual void ReconnectInFewSeconds()
	{
		isPortActive = false;
		m_isInit = false;
		m_cdToReconnect = 0f;
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

	protected void ReadData(byte[] _bytes)
	{
		if (!m_isInit)
			InitPortResolver();

		if (_bytes == null || _bytes.Length == 0)
		{
			m_cdToReconnect++;
			return;
		}

		m_cdToReconnect = 0f;
		isPortActive = true;

		m_bytes = _bytes;

		CurrentResolver.ResolveBytes(m_bytes);
	}

}