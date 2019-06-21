using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class TGDXTextCentre: TGDXBaseCentre {
    private StringBuilder m_stringBuilder;
    public Text debugText;

    public override void Init() {
        base.Init();
        m_stringBuilder = new StringBuilder();
    }

    public void WriteLine( string _line ) {
        m_stringBuilder.AppendLine( _line );
        debugText.text = m_stringBuilder.ToString();
    }

    public void Clear() {
        m_stringBuilder.Clear();
        debugText.text = string.Empty;
    }
}