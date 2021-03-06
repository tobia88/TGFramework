using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputReceiver {
    void MoveByPort( Vector3 _input );
    void MoveByTouch( Vector3 _pos );
    LMInputCtrl InputCtrl { get; }
}

public class LMInputCtrl: MonoBehaviour {
    [Range( 0f, 1f )]
    public float damp = 0.1f;
    public AxisOrder axisOrder;

    [Header( "Touch" )]
    public TouchDimension touchDimension;
    public LayerMask rayMaskFor3D;

    private Vector3 m_input;
    private IInputReceiver m_playerCtrl;

    public enum AxisOrder {
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX
    }

    public bool reverseX;
    public bool reverseY;

    public void OnInit( IInputReceiver _receiver ) {
        m_playerCtrl = _receiver;

        TGInputSetting.Touch.touchDimension = touchDimension;
        TGInputSetting.Touch.rayMaskForThreeD = rayMaskFor3D;
    }

    public void OnUpdate() {
        if( !TGInputSetting.IsPortActive ) {
            TouchInputUpdate();
        } else {
            PortInputUpdate();
        }
    }

    private void TouchInputUpdate() {
        var touch = TGInputSetting.Touch;

        if( touch == null )
            return;

        if( touch.IsTouched )
            m_playerCtrl.MoveByTouch( touch.CurrentPosition );


        if( touch.IsTouched )
            TGUtility.DrawHeatmap2D( touch.ScreenPosition );
    }

    private void PortInputUpdate() {
        var target = new Vector3();

        string strOrder = axisOrder.ToString().ToLower();

        if( TGData.DeviceType == "m7b" || TGData.DeviceType == "m7b2D" ) {
            target = TGInputSetting.GetValueFromEvalAxis();
        } else {
            target = TGInputSetting.GetValues();
        }

        target = target.Reorder( strOrder );

        target.x = Mathf.Clamp01( target.x );
        target.y = Mathf.Clamp01( target.y );
        target.z = Mathf.Clamp01( target.z );

        if( reverseX )
            target.x = 1.0f - target.x;

        if( reverseY )
            target.y = 1.0f - target.y;

        m_input.x = Mathf.Lerp( m_input.x, target.x, damp );
        m_input.y = Mathf.Lerp( m_input.y, target.y, damp );
        m_input.z = Mathf.Lerp( m_input.z, target.z, damp );

        m_playerCtrl.MoveByPort( m_input );
    }
}