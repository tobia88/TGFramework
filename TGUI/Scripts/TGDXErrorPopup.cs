using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum TGMessageTypes {
    Null,
    Warning,
    Error
}

public class TGDXErrorPopup: TGDXBaseCentre {
    public Text warningTxt;
    public TGDXButton normalBtnPrefab;
    public TGDXButton confirmBtnPrefab;
    public Transform buttonContainer;

    private TGDXButton[] _buttons;

    private System.Action<int> onClickCallback;

    private void OnEnable() {
        TGDXButton.onClick += OnClickButton;
    }

    private void OnDisable() {
        TGDXButton.onClick -= OnClickButton;
    }

    private void OnClickButton( TGDXButton _btn ) {
        onClickCallback( _btn.index );
    }

    public void PopupWithBtns( string msg, System.Action<int> callback ) {
        SetActive( true );

        warningTxt.text = msg;

        onClickCallback = callback;
    }

    public void PopupMessage( string _content ) {
        ClearButtons();

        warningTxt.text = _content;
    }

    public void PopupMessage( string _content, int _confirmIndex, Action<int> _callback, params string[] _options ) {
        ClearButtons();

        CreateButtons( _options, _confirmIndex );

        SetActive( true );

        warningTxt.text = _content;

        onClickCallback = _callback;
    }

    private void CreateButtons( string[] _options, int _confirmIndex ) {
        _buttons = new TGDXButton[_options.Length];

        for( int i = 0; i < _buttons.Length; i++ ) {
            var prefab = (i == _confirmIndex) ? confirmBtnPrefab : normalBtnPrefab;
            prefab = Instantiate( prefab );
            prefab.transform.SetParent( buttonContainer, false );
            prefab.Init( i, _options[i] );

            _buttons[i] = prefab;
        }
    }

    private void ClearButtons() {
        if( _buttons == null )
            return;

        foreach( var btn in _buttons ) {
            Destroy( btn.gameObject );
        }

        _buttons = null;
    }
}
