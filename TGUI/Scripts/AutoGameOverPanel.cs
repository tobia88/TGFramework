using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoGameOverPanel : MonoBehaviour
{
    private Animator m_animator;

    public TMPro.TextMeshProUGUI scoreTxt;
    public TMPro.TextMeshProUGUI countdownTxt;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void Show(int _score)
    {
        scoreTxt.text = _score.ToString();
        m_animator.SetBool("Show", true);
    }

    public void SetCountdownTxt(int _countdown)
    {
        countdownTxt.text = _countdown.ToString();
    }

    public void Exit()
    {
        m_animator.SetBool("Show", false);
    }
}
