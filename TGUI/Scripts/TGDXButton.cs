using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TGDXButton: MonoBehaviour {
    private Button m_button;

    public Text buttonTxt;
    public int index;

    public static event Action<TGDXButton> onClick;

    public void Init( int _id, string _content ) {
        index = _id;
        buttonTxt.text = _content;

        m_button = GetComponent<Button>();
        m_button.onClick.AddListener( () => onClick( this ) );
    }
}
