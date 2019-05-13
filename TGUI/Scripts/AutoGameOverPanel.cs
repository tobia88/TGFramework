using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoGameOverPanel : TGBasePanel
{
    public TMPro.TextMeshProUGUI scoreTxt;
    public TMPro.TextMeshProUGUI countdownTxt;
	public Sprite timeSpr;
	public Sprite missionSpr;
	public Image bgImg;

    public void SetScore(string score)
    {
        scoreTxt.text = score.ToString();
    }

    public void SetCountdownTxt(int _countdown)
    {
        countdownTxt.text = _countdown.ToString();
    }

	public void SetGameType(GameTypes gameType)
	{
        var spr = timeSpr;

        // 暂时不需要
		// var spr = (gameType == GameTypes.Missions) ? missionSpr : timeSpr;

		bgImg.sprite = spr;
	}
}
