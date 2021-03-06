﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LMGrindTableEmulator: LMBaseEmulator {
    private LMGrindTable m_grindTable;
    private Camera m_camera;
    private LMGrindTableEmulatorBtn[] m_btns;
    private RectTransform m_rectTrans;
    private int m_btnPressed;

    public int column, row;
    public LMGrindTableEmulatorBtn btnPrefab;
    public List<LMGrindTableEmulatorBtn> activatedButtons = new List<LMGrindTableEmulatorBtn>();

    public override void Init( LMBasePortInput input ) {
        m_grindTable = input as LMGrindTable;

        m_rectTrans = GetComponent<RectTransform>();

        column = LMGrindTable.ColumnCount;
        row = LMGrindTable.RowCount;

        CreateButtons();
    }

    public void SetBtnEnable( int x, int y ) {
        var btn = m_btns[y * column + x];
        activatedButtons.Add( btn );

        Refresh();
    }

    public void Refresh() {
        foreach( var btn in activatedButtons )
            btn.BtnState = EmuTableBtnStates.Waiting;
        activatedButtons[0].BtnState = EmuTableBtnStates.Start;
        activatedButtons[activatedButtons.Count - 1].BtnState = EmuTableBtnStates.End;
    }

    public void OnUpdate() {
        if( Input.GetKeyDown( KeyCode.Space ) ) {
            Reset();
            if( LMGrindTable.onTestFinished != null )
                LMGrindTable.onTestFinished( true );
        }
    }

    public void Reset() {
        m_btnPressed = 0;

        Debug.Log( "Reset" );
        foreach( var btn in m_btns )
            btn.BtnState = EmuTableBtnStates.Null;

        activatedButtons.Clear();
    }

    public void Restart() {
        if( activatedButtons == null )
            return;

        activatedButtons[0].BtnState = EmuTableBtnStates.Start;
        for( int i = 1; i < activatedButtons.Count - 1; i++ ) {
            activatedButtons[i].BtnState = EmuTableBtnStates.Waiting;
        }
        activatedButtons[activatedButtons.Count - 1].BtnState = EmuTableBtnStates.End;

        m_btnPressed = 0;
    }

    public void OnBtnClick( LMGrindTableEmulatorBtn btn ) {
        if( activatedButtons.Contains( btn ) ) {
            m_btnPressed++;

            if( LMGrindTable.onTurnOffLight != null ) {
                LMGrindTable.onTurnOffLight( new GrindNode() { x = btn.x, y = btn.y } );
            }

            Debug.Log( "Btn Pressed: " + m_btnPressed );

            if( btn.BtnState == EmuTableBtnStates.End ) {
                bool result = m_btnPressed >= 5;

                if( !result )
                    Restart();
                else
                    Reset();

                // 如果按钮点击量大于等于5，则表示训练通过，反之从来一次
                if( LMGrindTable.onTestFinished != null )
                    LMGrindTable.onTestFinished( result );
            } else {
                btn.BtnState = EmuTableBtnStates.Pressed;
            }
        }
    }

    private void SetTransform( LMGrindTableEmulatorBtn btn, int x, int y ) {
        var r = m_rectTrans.rect;

        var gridSize = new Vector2( r.width / column, r.height / row );
        var spacing = gridSize * 0.1f;

        // 从左上角开始生成
        var origin = new Vector3( r.width * -0.5f, r.height * 0.5f );

        // 偏移量，以确保按键生成在网格里
        origin.x += gridSize.x * 0.5f;
        origin.y -= gridSize.y * 0.5f;

        // var stepX = r.width / ( column - 1 );
        // var stepY = r.height / ( row - 1 );

        var rect = btn.transform as RectTransform;
        var p = new Vector2( origin.x + gridSize.x * x, origin.y - gridSize.y * y );
        rect.anchoredPosition = p;
        rect.sizeDelta = gridSize - spacing;
    }

    private void CreateButtons() {
        m_btns = new LMGrindTableEmulatorBtn[column * row];

        for( int y = 0; y < row; y++ ) {
            for( int x = 0; x < column; x++ ) {
                m_btns[y * column + x] = CreateButton( x, y );
            }
        }
    }

    private LMGrindTableEmulatorBtn CreateButton( int x, int y ) {
        var btn = Instantiate( btnPrefab );

        btn.transform.SetParent( transform, true );
        btn.Init( this, x, y );
        SetTransform( btn, x, y );

        return btn;
    }
}