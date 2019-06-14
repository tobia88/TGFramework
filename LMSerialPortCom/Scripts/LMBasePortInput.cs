using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

public interface IPortReceiver {
    void OnReceivePort( SerialPort port );
}

[System.Serializable]
public class KeyInputConfig {
    public KeyPortData[] keys;

    public static KeyInputConfig GetInstance() {
        var retval = LMFileWriter.ReadJSON<KeyInputConfig>( TGPaths.KeyInputSetting );
        if( retval == null ) {
            throw new Exception( TGPaths.KeyInputSetting  + " 并不存在" );
        }
        return retval;
    }

    public KeyPortData GetKeyportData( string keyName ) {
        foreach( KeyPortData k in keys ) {
            if( k.name.FirstOrDefault( n => n == keyName ) != null )
                return k;
        }

        return null;
    }
}

public abstract class LMBasePortInput {
    protected float m_cdTick;
    protected int m_byteLength;
    protected bool m_isConnected;

    public byte[] Bytes { get; protected set; }
    public TGController controller { get; private set; }
    public bool IsPortActive { get; protected set; }
    public virtual bool IsConnected { get { return IsPortActive && !IsPortWriting && m_isConnected; } }
    public bool IsPortWriting { get; protected set; }
    public LMBasePortResolver CurrentResolver { get; protected set; }
    public KeyPortData KeyportData { get; protected set; }
    public string ErrorTxt { get; protected set; }
    public bool HasData { get; protected set; }
    public float ConnectLimit { get { return 5f; } }
    public LMBaseEmulator Emulator { get; private set; }
    public bool IsTesting { get; protected set; }

    public System.Action<byte[]> onReceiveDataCallback;

    private bool CountdownToReconnect() {
        m_cdTick += Time.deltaTime;
        return m_cdTick >= ConnectLimit;
    }

    public virtual void Init( TGController _controller, KeyPortData keyportData ) {
        controller = _controller;
        KeyportData = keyportData;
    }

    public virtual IEnumerator OnStart( LMBasePortResolver resolver = null ) {
        if( !OpenPort() )
            yield break;

        Debug.Log( "端口正常运作，准备接收数据" );

		// 获取解释器，并初始化
        CurrentResolver = ( resolver != null ) ? resolver : GetProperResolver( KeyportData );

        if( CurrentResolver != null )
            CurrentResolver.Init( this );

		// 尝试连接
        yield return controller.StartCoroutine( TestConnect() );

        if( CurrentResolver != null )
            yield return controller.StartCoroutine( OnStartResolver() );

        if( !IsConnected ) {
            Close();
            ErrorTxt = "连接失败，请检查设备是否正确连接";
        }
    }

    public abstract bool OpenPort();
    public abstract void Write( byte[] bytes );

    public virtual void Recalibration() {
        CurrentResolver.Recalibration();
    }

    public void Write( string code, bool isHex = true ) {
        byte[] bytes = ( isHex ) ? TGUtility.HexToByteArray( code ) : Encoding.ASCII.GetBytes( code );
        Debug.Log( "Write Port: " + code + ", Converted into: " + BitConverter.ToString( bytes ) );
        Write( bytes );
    }

    public virtual void Close() {
        if( CurrentResolver != null && IsPortActive )
            CurrentResolver.Close();

        IsPortActive = false;
        Reset();
    }

    public virtual bool OnUpdate() {
        if( !IsPortActive || IsPortWriting )
            return false;

        if( CountdownToReconnect() ) {
            Debug.LogWarning( "Time over than limit: " + ConnectLimit + ", prepare for reconnection" ); ;
            ReconnectInFewSeconds();
        }

        return true;
    }

    public virtual float GetValue( int index ) {
        if( CurrentResolver == null )
            return 0f;

        return CurrentResolver.GetValue( index );
    }

    public virtual float GetRawValue( int index ) {
        if( CurrentResolver == null )
            return 0f;

        return CurrentResolver.GetRawValue( index );
    }

    public virtual void SetTest( bool testing ) {
        Emulator = GameObject.FindObjectOfType<LMBaseEmulator>();

        if( Emulator != null ) {
            Emulator.gameObject.SetActive( testing );

            if( testing )
                Emulator.Init( this );
        }

        IsTesting = testing;

    }

    public virtual IEnumerator TestConnect() {
        for( int i = 0; i < 5; i++ ) {
            yield return new WaitForSeconds( 1f );

            IsPortActive = m_isConnected && !IsPortWriting;

            if( IsPortActive )
                yield break;
        }
    }

    protected virtual IEnumerator OnStartResolver() {
        if( !IsConnected )
            yield break;

        CurrentResolver.Start();

        yield return controller.StartCoroutine( TestConnect() );
    }

    protected virtual void ReconnectInFewSeconds() {
        Reset();
    }

    protected void Reset() {
        // m_isInit = false;
        m_cdTick = 0f;
        IsPortWriting = false;
        m_isConnected = false;
    }

    // 获取合适的解释器
    protected LMBasePortResolver GetProperResolver( KeyPortData portData ) {
        LMBasePortResolver retval = null;

        Debug.Log( "Port Data Type: " + portData.type );

        if( portData.type == "jy901" ) {
            retval = new JY901();
        } else if( portData.type == "m7b" || portData.type == "m7b2D" ) {
            retval = new Leadiy_M7B();
        } else if( portData.type == "key" || portData.type == "key2D" ) {
            retval = new LMKeyResolver();
        }

        return retval;
    }

    // 处理数据
    protected virtual void OnHandleData( Byte[] bytes ) {
        if( onReceiveDataCallback != null )
            onReceiveDataCallback( bytes );

        Bytes = bytes;

        HasData = Bytes != null && Bytes.Length > 0;

        if( !HasData )
            return;

        m_cdTick = 0f;
        m_isConnected = true;

        ResolveBytes( Bytes );
    }

    protected virtual void ResolveBytes( Byte[] bytes ) {
        if( CurrentResolver != null )
            CurrentResolver.ResolveBytes( bytes );
    }
}