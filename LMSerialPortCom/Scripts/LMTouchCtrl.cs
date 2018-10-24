using System;
using System.Collections.Generic;
using UnityEngine;

public class LMTouchCtrl : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_lastInput;

    public Action<Vector3> onTouchCallback;

    public enum TouchDimension
    {
        TwoD,
        ThreeD
    }

    public LayerMask rayMaskForThreeD;

    public TouchDimension touchDimension;

    private void Update()
    {
        if (touchDimension == TouchDimension.TwoD)
            Get2DTouchValue();

        else
            Get3DTouchValue();
    }

    private void Get2DTouchValue()
    {
        Camera cam = Camera.main;

        if (CheckIsPressed())
        {
            Vector3 screenPos = GetSingleInputPos();

            if (onTouchCallback != null)
            {
                onTouchCallback(cam.ScreenToWorldPoint(screenPos));
            }
        }
    }

    private void Get3DTouchValue()
    {
        Camera cam = Camera.main;

        if (CheckIsPressed())
        {
            Vector3 screenPos = GetSingleInputPos();
            Ray ray = cam.ScreenPointToRay(screenPos);

            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * 1000f, Color.red);

            if (Physics.Raycast(ray, out hit, rayMaskForThreeD))
            {
                if (onTouchCallback != null)
                    onTouchCallback(hit.point);
            }
        }
    }

    private bool CheckIsPressed()
    {
        return Input.touchCount > 0 || Input.GetMouseButton(0);
    }

    private Vector3 GetSingleInputPos()
    {
        if (Input.touchCount > 0)
        {
            m_lastInput = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButton(0))
        {
            m_lastInput = Input.mousePosition;
        }

        return m_lastInput;
    }
}