using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TGMessageTypes
{
    Null,
    Warning,
    Error
}

public class TGDXErrorPopup : TGDXBaseCentre
{
    public Text warningTxt;
    public Button confirmBtn;
    public Button exitBtn;

    private System.Action<int> onClickCallback;

    public override void OnInit(TGController _controller)
    {
        base.OnInit(_controller);
        exitBtn.onClick.AddListener(() => onClickCallback(0));
        confirmBtn.onClick.AddListener(() => onClickCallback(1));
    }

    public void SetButtonActive(bool active)
    {
        confirmBtn.gameObject.SetActive(active);
        exitBtn.gameObject.SetActive(active);
    }

    public void PopupWithBtns(string msg, System.Action<int> callback)
    {
        SetActive(true);
        SetButtonActive(true);

        warningTxt.text = msg;

        onClickCallback = callback;
    }

    public void PopupMessage(string msg)
    {
        SetActive(true);
        SetButtonActive(false);
        warningTxt.text = msg;
    }
}
