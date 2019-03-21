using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TGDXLoadingPanel :TGDXBaseCentre
{
    public Image image;
    public override void OnInit(TGController _controller)
    {
        base.OnInit(_controller);
        SetActive(true);
    }
    public override void SetActive(bool _active)
    {
        base.SetActive(_active);
    }

}
