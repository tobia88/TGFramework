using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TGDXLoadingPanel: TGDXBaseCentre {
    public Image image;
    public override void Init() {
        base.Init();
        SetActive( true );
    }
    public override void SetActive( bool _active ) {
        base.SetActive( _active );
    }

}
