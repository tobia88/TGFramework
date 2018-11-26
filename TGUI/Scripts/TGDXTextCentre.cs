using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class TGDXTextCentre : MonoBehaviour
{
	private StringBuilder m_stringBuilder;
	public Text debugText;
	public bool isActive;

	public void OnInit(TGController _controller)
	{
		m_stringBuilder = new StringBuilder();
		SetActive(false);
	}

	public void SetActive(bool _active)
	{
		isActive = _active;

		gameObject.SetActive(isActive);
	}

	public void WriteLine(string _line)
	{
		m_stringBuilder.AppendLine(_line);
		debugText.text = m_stringBuilder.ToString();
	}

	public void Clear()
	{
		m_stringBuilder.Clear();
		debugText.text = string.Empty;
	}
}