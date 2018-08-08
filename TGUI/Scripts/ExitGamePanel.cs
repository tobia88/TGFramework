using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitGamePanel : MonoBehaviour
{
	public Button confirmBtn;
	public Button cancelBtn;
	public System.Action onFinishClosePanel;
	private Animator m_animator;

	private void Awake()
	{
		m_animator = GetComponent<Animator>();
	}

	public void Show()
	{
		m_animator.SetBool("OnShow", true);
	}

	public void Exit()
	{
		m_animator.SetBool("OnShow", false);
	}

	public void OnFinishExit()
	{
		if (onFinishClosePanel != null)
			onFinishClosePanel();
	}
}
