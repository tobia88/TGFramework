using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TGDebug: TGBaseBehaviour {
    private static TGDebug m_Instance;
    public TGDXErrorPopup errorPopup;
    public TGDXHeatmapPanel heatmapPanel;
    
	public static void ErrorBox( string _content, int _confirmIndex, Action<int> _callback, params string[] _options ) {
        m_Instance.errorPopup.PopupMessage( _content, _confirmIndex, _callback, _options );
	}

    public static void MessageBox( string _content ) {
        m_Instance.errorPopup.PopupMessage( _content );
    }

    public static void ClearMessageBox() {
        m_Instance.errorPopup.SetActive( false );
    }

    public void Init() {
        errorPopup.Init();
        heatmapPanel.Init();

        m_Instance = this;
    }
}
