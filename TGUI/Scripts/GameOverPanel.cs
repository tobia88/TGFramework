using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    private Animator m_animator;

    public Button quitBtn;
    public Button restartBtn;
    public TMPro.TextMeshProUGUI scoreTxt;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void Show(int _score)
    {
        scoreTxt.text = _score.ToString();
        m_animator.SetBool("Show", true);
    }

    public void Exit()
    {
        m_animator.SetBool("Show", false);
    }
}
