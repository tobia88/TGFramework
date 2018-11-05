using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPanel : TGBasePanel
{
	public Button confirmBtn;

	public System.Action onFinishClosePanel;

	public Image tutorialImg;

	public void SetImage(Sprite _spr)
	{
		tutorialImg.sprite = _spr;
	}

	public void OnFinishExit()
	{
		if (onFinishClosePanel != null)
			onFinishClosePanel();
	}
}