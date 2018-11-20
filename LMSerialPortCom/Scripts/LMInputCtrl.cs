using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputReceiver
{
    void MoveByPort(Vector3 _input);
    void MoveByTouch(Vector2 _pos);
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

    private void Awake()
    {
        m_playerCtrl = GetComponent<IInputReceiver>();
    }

    private void Update()
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
        if (touch.IsTouched)
            m_playerCtrl.MoveByTouch(touch.CurrentPosition);
    }

    private void PortInputUpdate(TGInputSetting input)
    {
        var target = new Vector3();

        string strOrder = axisOrder.ToString().ToLower();

        if (input.DeviceType == "m7b")
        {
            if (strOrder[0] == 'x')
                target.x = input.GetValueFromEvalAxis();
            
            else if (strOrder[0] == 'y')
                target.y = input.GetValueFromEvalAxis();

            else if (strOrder[0] == 'z')
                target.z = input.GetValueFromEvalAxis();
        }
        else
        {
            target = input.GetValues(strOrder);
        }

        target.x = Mathf.Clamp(target.x, 0f, 1f);
        target.y = Mathf.Clamp(target.y, 0f, 1f);
        target.z = Mathf.Clamp(target.z, 0f, 1f);

        m_input.x = Mathf.Lerp(m_input.x, target.x, damp);
        m_input.y = Mathf.Lerp(m_input.y, target.y, damp);
        m_input.z = Mathf.Lerp(m_input.z, target.z, damp);

        m_playerCtrl.MoveByPort(m_input);
    }
}