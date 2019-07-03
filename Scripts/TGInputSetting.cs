// #define USE_TOUCH_IF_DISCONNECT

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TGInputSetting: TGBaseManager {
    public static LMTouchCtrl Touch { get; private set; }
    public static LMBasePortInput PortInput { get; private set; }
    public static bool IsPortActive {
        get { return ( PortInput == null ) ? false : PortInput.IsConnected; }
    }

    public static KeyPortData KeyportData { get; private set; }

    public override IEnumerator StartRoutine() {

        Touch = new LMTouchCtrl(); 

        KeyportData = TGData.KeyPortData;

        if( KeyportData == null ) {
            m_controller.ErrorQuit( "训练器材 " + TGData.DeviceName + "不存在！" );
        }

        // 检测是否开启触屏模式
        // 如果不是则开始安排线程接受端口数据
        if( KeyportData.type != "touch" ) {
            yield return StartCoroutine( ConnectDeviceRoutine() );
        }

        // 打开热图
        m_controller.SetHeatmapEnable( KeyportData.heatmap );

        m_controller.ProgressValue += 0.1f;

        Debug.Log( "输入设备配置完成" );
    }

    private IEnumerator ConnectDeviceRoutine() {

        if( TGData.IsTesting )
            Debug.Log( "开启测试模式" );

        // FIXME: Temperory
        if( KeyportData.type == "m7b" && TGData.evalData.isFullAxis ) {
            KeyportData.type += "2D";
        }

        PortInput = GetProperInput();

        if( TGData.IsTesting )
            yield break;

        while( !PortInput.IsPortActive ) {
            yield return StartCoroutine( PortInput.OnStart() );

            if( !PortInput.IsPortActive ) {
                int result = -1;

                TGDebug.ErrorBox( PortInput.ErrorTxt, 1, i => result = i, "取消", "确定" );
                yield return new WaitUntil( () => result >= 0 );

                if( result == 0 ) {
                    m_controller.Quit();
                    yield break;
                }

                TGDebug.MessageBox( "重连中...");

                yield return new WaitForSecondsRealtime( 0.1f );
            }
        }

        TGDebug.ClearMessageBox();
    }

    public override IEnumerator EndRoutine() {
        UnplugInput();
        yield return 1;
    }

    public void UnplugInput() {
        if( PortInput != null )
            PortInput.Close();
    }

    public void OnGameStart() {
        if( PortInput != null )
            PortInput.SetTest( TGData.IsTesting );
    }

    public override void ForceClose() {
        UnplugInput();
    }

    private LMBasePortInput GetProperInput() {
        //FIXME: Temperory
        if( KeyportData.type == "CASMB" ) {
            var retval = new LMGrindTable();
            retval.Init( m_controller,
                KeyportData,
                TGGameConfig.GetValue( "端口", -1 ) );
            return retval;
        }

        int udp = TGGameConfig.GetValue( "UDP", -1 );

        if( udp >= 0 ) {
            var retval = new LMInput_UDP();
            retval.Init( m_controller, KeyportData, udp );
            Debug.Log( "准备衔接UDP设备" );
            return retval;
        } else {
            var retval = new LMInput_Port();
            retval.Init( m_controller,
                KeyportData,
                TGGameConfig.GetValue( "端口", -1 ) );
            Debug.Log( "准备衔接端口设备" );
            return retval;
        }
    }

    public static void SetPressureLevel( int level ) {
        float[] arr = KeyportData.levels;

        if( arr == null || arr.Length == 0 ) {
            Debug.LogWarning( "压力等级尚未设置" );
            return;
        }

        float pressure = arr[level - 1];
        Debug.Log( "压力比例设置为: " + pressure );

        if( PortInput.CurrentResolver != null ) {
            PortInput.CurrentResolver.SetPressureRatio( pressure );
        }
    }

    public static Vector3 GetValueFromEvalAxis() {
        if( !IsPortActive )
            return Vector3.zero;

        var data = TGData.evalData;
        var valueAxis = data.valueAxis;

        Vector3 values = GetValues();
        Vector3 retval = values.Reorder( valueAxis.ToString() );

        if( !data.isFullAxis ) {
            retval.y = retval.z = 0f;
        }

        return retval;
    }

    public void OnUpdate() {
        if( Touch != null )
            Touch.OnUpdate();

        if( PortInput != null )
            PortInput.OnUpdate();
    }

    public static Vector3 GetValues() {
        Vector3 retval = Vector3.zero;
        if( IsPortActive ) {
            retval.x = PortInput.GetValue( 0 );
            retval.y = PortInput.GetValue( 1 );
            retval.z = PortInput.GetValue( 2 );
        }

        return retval;
    }

    public static Vector3 GetRawValues() {
        Vector3 retval = Vector3.zero;
        if( IsPortActive ) {
            retval.x = PortInput.GetRawValue( 0 );
            retval.y = PortInput.GetRawValue( 1 );
            retval.z = PortInput.GetRawValue( 2 );
        }

        return retval;
    }

    public static void Recalibration() {
        if( IsPortActive )
            PortInput.Recalibration();
    }
}