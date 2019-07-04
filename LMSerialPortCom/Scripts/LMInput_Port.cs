using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System;
using UnityEngine;

public class LMInput_Port: LMBasePortInput, IPortReceiver {
    public SerialPort Port { get; private set; }
    public LMSerialPortCtrl SerialPortCtrl { get; private set; }
    public PortInfo portInfo;

    public override bool OpenPort() {
        bool retval = SerialPortCtrl.Open( portInfo, this, true );

        if( !retval )
            ErrorTxt = portInfo.comName + "无法打开，请检查设备后再重试";

        return retval;
    }

    public virtual void Init( TGController _controller, KeyPortData keyportData, int _com ) {
        base.Init( _controller, keyportData );

        SerialPortCtrl = new LMSerialPortCtrl();

        portInfo = new PortInfo();

        portInfo.comName = "COM" + _com;
        portInfo.baudRate = 115200;
    }

    public override IEnumerator OnStart( LMBasePortResolver resolver = null ) {
        if( !SerialPortCtrl.CheckPortAvailable( portInfo.comName ) ) {
            ErrorTxt = portInfo.comName + "不存在，请修改正确的COM再重试";
            yield break;
        }

        yield return controller.StartCoroutine( base.OnStart( resolver ) );
    }

    public override void Write( byte[] bytes ) {
        if( Port != null && Port.IsOpen ) {
            controller.StartCoroutine( PortWriteRoutine( bytes ) );
        }
    }

    private IEnumerator PortWriteRoutine( byte[] bytes ) {
        while( !IsPortWriting ) {
            try {
                Port.Write( bytes, 0, bytes.Length );
                IsPortWriting = true;
            } catch( System.TimeoutException ex ) {
                Debug.LogWarning( ex );
                Debug.Log( "Restart port write routine" );
            }
        }

        yield return new WaitForSeconds( 0.1f );

        m_isConnected = false;
        yield return new WaitUntil( () => m_isConnected );

        Debug.Log( "Write Finished" );
        IsPortWriting = false;
    }

    public override void Close() {
        base.Close();

        if( SerialPortCtrl != null )
            SerialPortCtrl.Close();
    }

    public void OnReceivePort( SerialPort _port ) {
        Port = _port;

        byte[] tmpBytes = null;

        if( Port.IsOpen ) {
            m_byteLength = Port.BytesToRead;

            if( m_byteLength > 0 ) {
                tmpBytes = new byte[m_byteLength];
                Port.Read( tmpBytes, 0, m_byteLength );
            }
        }

        OnHandleData( tmpBytes );
    }

    protected override void ReconnectInFewSeconds() {
        base.ReconnectInFewSeconds();
        SerialPortCtrl.Close();
        controller.StartCoroutine( ReconnectDelay() );
    }

    private IEnumerator ReconnectDelay() {
        bool result = false;

        while( !result ) {
            yield return new WaitForSeconds( 5 );
            result = OpenPort();
        }
    }
}