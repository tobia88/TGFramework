using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum EmuTableBtnStates
{
    Null,
    Waiting,
    Pressed
}

public class LMGrindTableEmulatorBtn : MonoBehaviour, IPointerClickHandler
{
    private EmuTableBtnStates m_btnState;
    private Image m_image;
    private LMGrindTableEmulator m_emulator;
    
    public int x;
    public int y;

    public EmuTableBtnStates BtnState
    {
        get { return m_btnState; }
        set
        {
            if (m_btnState != value)
            {
                SetBtnValue(value);
            }
        }
    }

    public void Init(LMGrindTableEmulator emulator, int x, int y)
    {
        m_emulator = emulator;
        m_image = GetComponent<Image>();

        this.x = x;
        this.y = y;

        m_image.color = GetColor(EmuTableBtnStates.Null);
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (m_btnState != EmuTableBtnStates.Waiting)
            return;

        m_emulator.OnBtnClick(this);
    }

    public void SetBtnValue(EmuTableBtnStates state)
    {
        m_btnState = state;
        m_image.color = GetColor(m_btnState);
    }

    private Color GetColor(EmuTableBtnStates state)
    {
        if (m_btnState == EmuTableBtnStates.Pressed) return Color.green;
        if (m_btnState == EmuTableBtnStates.Waiting) return Color.yellow;
        return Color.gray;
    }
}
