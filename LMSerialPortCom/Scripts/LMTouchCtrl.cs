using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public enum TouchDimension {
    TwoD,
    ThreeD
}

public class LMTouchCtrl {
    [SerializeField]
    private Vector3 m_lastInput;
    private Vector3 m_currentPosition;
    private bool m_isTouched;

    public Action onTouchDown;
    public Action onTouchUp;

    public Vector3 LastPosition { get; private set; }
    public Vector3 ScreenPosition { get; private set; }
    public Vector3 CurrentPosition {
        get { return m_currentPosition; }
        set {
            LastPosition = m_currentPosition;
            m_currentPosition = value;
        }
    }

    public bool IsTouched {
        get { return m_isTouched; }
        set {
            if( m_isTouched != value ) {
                m_isTouched = value;

                if( m_isTouched ) {
                    if( onTouchDown != null )
                        onTouchDown();
                } else {
                    if( onTouchUp != null )
                        onTouchUp();
                }
            }
        }
    }

    public LayerMask rayMaskForThreeD;

    public TouchDimension touchDimension;

    public void OnUpdate() {
        if( EventSystem.current.currentSelectedGameObject != null )
            return;

        bool isPressed = CheckIsPressed();

        if( isPressed ) {
            if( touchDimension == TouchDimension.TwoD )
                Get2DTouchValue();

            else
                Get3DTouchValue();
        }

        IsTouched = isPressed;
    }

    private void Get2DTouchValue() {
        Camera cam = Camera.main;

        if( cam == null )
            return;

        ScreenPosition = GetSingleInputPos();

        CurrentPosition = cam.ScreenToWorldPoint( ScreenPosition );
    }

    private void Get3DTouchValue() {
        Camera cam = Camera.main;

        ScreenPosition = GetSingleInputPos();
        Ray ray = cam.ScreenPointToRay( ScreenPosition );

        RaycastHit hit;

        Debug.DrawRay( ray.origin, ray.direction * 1000f, Color.red );

        if( Physics.Raycast( ray, out hit, rayMaskForThreeD ) ) {
            CurrentPosition = hit.point;
        }
    }

    private bool CheckIsPressed() {
        return Input.touchCount > 0 || Input.GetMouseButton( 0 );
    }

    private Vector3 GetSingleInputPos() {
        if( Input.touchCount > 0 ) {
            m_lastInput = Input.GetTouch( 0 ).position;
        } else if( Input.GetMouseButton( 0 ) ) {
            m_lastInput = Input.mousePosition;
        }

        return m_lastInput;
    }
}