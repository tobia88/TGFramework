using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoGameOverPanel : TGBasePanel
{
    public TMPro.TextMeshProUGUI scoreTxt;
    public TMPro.TextMeshProUGUI countdownTxt;

    public void SetScore(int _score)
    {
        scoreTxt.text = _score.ToString();
    }

    public void SetCountdownTxt(int _countdown)
    {
        countdownTxt.text = _countdown.ToString();
    }
}
