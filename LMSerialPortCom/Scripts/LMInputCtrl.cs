using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputReceiver
{
    void MoveByPort(Vector3 _input);
    void MoveByTouch(Vector3 _pos);
    LMInputCtrl InputCtrl {get;}
}

public class LMInputCtrl : MonoBehaviour
{
    [Range(0f, 1f)]
    public float damp = 0.1f;
    public AxisOrder axisOrder;

    private Vector3 m_input;
    private IInputReceiver m_playerCtrl;

    public enum AxisOrder
    {
        XYZ,
        XZY,
        YXZ,
        YZX,
        ZXY,
        ZYX
    }

    public bool reverseX;
    public bool reverseY;

    public void OnInit(IInputReceiver _receiver)
    {
        m_playerCtrl = _receiver;
    }

    public void OnUpdate()
    {
        var input = TGController.Instance.inputSetting;

        if (!input.IsPortActive)
        {
            TouchInputUpdate(input.touchCtrl);
        }
        else
        {
            PortInputUpdate(input);
        }
    }

    private void TouchInputUpdate(LMTouchCtrl touch)
    {
        if (touch == null)
            return;

        if (touch.IsTouched)
            m_playerCtrl.MoveByTouch(touch.CurrentPosition);


        if (touch.IsTouched)
            TGUtility.DrawHeatmap2D(touch.ScreenPosition);
    }

    private void PortInputUpdate(TGInputSetting input)
    {
        var target = new Vector3();

        string strOrder = axisOrder.ToString().ToLower();

        if (input.DeviceType == "m7b" || input.DeviceType == "m7b2D")
        {
            target = input.GetValueFromEvalAxis();
        }
        else
        {
            target = input.GetValues();
        }

        target = target.Reorder(strOrder);

        target.x = Mathf.Clamp01(target.x);
        target.y = Mathf.Clamp01(target.y);
        target.z = Mathf.Clamp01(target.z);

        if (reverseX)
            target.x = 1.0f - target.x;

        if (reverseY)
            target.y = 1.0f - target.y;

        m_input.x = Mathf.Lerp(m_input.x, target.x, damp);
        m_input.y = Mathf.Lerp(m_input.y, target.y, damp);
        m_input.z = Mathf.Lerp(m_input.z, target.z, damp);

        // TGController.Instance.WriteLine("Input After: " + m_input.ToString());

        m_playerCtrl.MoveByPort(m_input);
    }

    // private Vector3 M7BUpdate(TGInputSetting input, string strOrder)
    // {
    //     Vector3 retval = Vector3.zero;

    //     Vector3 val = input.GetValueFromEvalAxis();
    //     val.x = Mathf.Clamp01(val.x);
    //     val.y = Mathf.Clamp01(val.y);
    //     val.z = Mathf.Clamp01(val.z);

    //     if (TGController.Instance.evaluationSetupData.reverse)
    //         val.x = 1f - val.x;

    //     if (strOrder[0] == 'x')
    //         retval.x = val;

    //     else if (strOrder[0] == 'y')
    //         retval.y = val;

    //     else if (strOrder[0] == 'z')
    //         retval.z = val;

    //     return retval;
    // }
}