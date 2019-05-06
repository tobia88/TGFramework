using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMGrindTableEmulator : LMBaseEmulator
{
    private LMGrindTable m_grindTable;
    private Camera m_camera;
    private LMGrindTableEmulatorBtn[] m_btns;
    private RectTransform m_rectTrans;

    public int column, row;
    public LMGrindTableEmulatorBtn btnPrefab;
    public List<LMGrindTableEmulatorBtn> activatedButtons = new List<LMGrindTableEmulatorBtn>();

    public override void Init(LMBasePortInput input)
    {
        m_grindTable = input as LMGrindTable;

        m_rectTrans = GetComponent<RectTransform>();

        column = m_grindTable.ColumnCount;
        row = m_grindTable.RowCount;

        CreateButtons();
    }

    public void SetBtnEnable(int x, int y, EmuTableBtnStates btnState)
    {
        var btn = m_btns[y * column + x];
        btn.BtnState = btnState;
        activatedButtons.Add(btn);
    }

    public void Reset()
    {
        Debug.Log("Reset");
        foreach (var btn in m_btns)
            btn.BtnState = EmuTableBtnStates.Null;
    }

    public void OnBtnClick(LMGrindTableEmulatorBtn btn)
    {
        if (activatedButtons.Contains(btn))
        {
            btn.BtnState = EmuTableBtnStates.Pressed;

            if (m_grindTable.onTurnOffLight != null)
            {
                m_grindTable.onTurnOffLight(m_grindTable.NodeToVector(btn.x, btn.y));
            }

            activatedButtons.Remove(btn);

            Debug.Log("Button Left: " + activatedButtons.Count);

            if (activatedButtons.Count <= 0)
            {
                Reset();

                if (m_grindTable.onTestFinished != null)
                    m_grindTable.onTestFinished();
            }
        }
    }

    void Update()
    {
        if (m_btns == null)
            return;

        for (int i = 0; i < m_btns.Length; i++)
        {
            int x = i % column;
            int y = i / column;

            SetTransform(m_btns[i], x, y);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (m_grindTable.onTestFinished != null)
                m_grindTable.onTestFinished();
        }
    }

    private void SetTransform(LMGrindTableEmulatorBtn btn, int x, int y)
    {
        var r = m_rectTrans.rect;

        // Start from top left
        var origin = new Vector3(r.width * -0.5f, r.height * 0.5f);

        var stepX = r.width / (column - 1);
        var stepY = r.height / (row - 1);

        var rect = btn.transform as RectTransform;
        var p = new Vector2(origin.x + stepX * x, origin.y - stepY * y);
        rect.anchoredPosition = p;
        rect.sizeDelta = new Vector2(stepX * 0.8f, stepY * 0.8f);
    }

    private void CreateButtons()
    {
        m_btns = new LMGrindTableEmulatorBtn[column * row];

        for (int y = 0; y < row; y++)
        {
            for (int x = 0; x < column; x++)
            {
                m_btns[y * column + x] = CreateButton(x, y);
            }
        }
    }

    private LMGrindTableEmulatorBtn CreateButton(int x, int y)
    {
        var btn = Instantiate(btnPrefab);

        btn.transform.SetParent(transform, true);
        btn.Init(this, x, y);
        SetTransform(btn, x, y);

        return btn;
    }
}