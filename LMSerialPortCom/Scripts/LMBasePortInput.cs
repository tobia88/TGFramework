using UnityEngine;
using System;
using System.IO.Ports;

public interface IPortReceiver
{
    void OnReceivePort(SerialPort port);
}
public abstract class LMBasePortInput : MonoBehaviour, IPortReceiver
{
    protected LMSerialPortCtrl m_serialPortCtrl;
    protected LMFileWriter m_fileWriter;
    protected byte[] m_bytes;
    protected string m_getString;
    public string debugPath = "port.txt";
    public TGController controller {get; private set;}

    public string ErrorTxt
    {
        get; protected set;
    }

    public PortInfo portInfo;
    public bool isPortActive = false;

    public virtual void SetDefaultValue(string key, object val) { }

    public virtual bool OnStart()
    {
        m_serialPortCtrl = FindObjectOfType<LMSerialPortCtrl>();
        controller = TGController.Instance;
        m_fileWriter = controller.fileWriter;

        if (!m_serialPortCtrl.CheckPortAvailable(portInfo.comName))
        {
            ErrorTxt = portInfo.comName + " Doesn't Exist!";
            return false;
        }

        isPortActive = true;
        m_serialPortCtrl.Open(portInfo, this, true);

        return true;
    }

    public void OnReceivePort(SerialPort _port)
    {
        try
        {
            if (_port.IsOpen)
            {
				ReceiveActivePortData(_port);

                int byteLength = _port.BytesToRead;
                
                if (byteLength <= 0)
                    return;

                m_bytes = new byte[byteLength];
                _port.Read(m_bytes, 0, byteLength);

                ReceiveBytes(m_bytes);
            }
        }
        catch (Exception _ex)
        {
            ErrorTxt = _ex.ToString();
            Debug.LogWarning(_ex);
        }
    }

    public virtual float GetValue(string key, float min, float max, float remapMin, float remapMax) {return -1f;}

    public virtual void Recalibration(){}
    
    protected virtual void ReceiveActivePortData(SerialPort _port) { }

    protected virtual void ReceiveBytes(byte[] _bytes) { }
}
