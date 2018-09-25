using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCtrl : MonoBehaviour
{
	public static InputCtrl Instance;
	public bool usePort;

	private LMBasePortInput m_portInput;
	private float m_min, m_max;

	public bool IsAvailable
	{
		get { return m_portInput != null && m_portInput.isPortActive && usePort; }
	}

	public void OnStart()
	{
		Instance = this;

		m_portInput = TGController.Instance.inputSetting.CurrentPortInput;

		if (m_portInput != null && m_portInput.isPortActive)
		{
			m_min = TGController.Instance.gameConfig.GetValue("旋前最大距离", -1);
			m_max = TGController.Instance.gameConfig.GetValue("旋后最大距离", -1);

			if (m_portInput != null && m_portInput.isPortActive)
			{
				m_portInput.SetDefaultValue("x", new float[] { m_min, m_max, 0 });
			}

			Invoke("Recalibration", 1f);
		}
	}

	private void Recalibration() {
		m_portInput.Recalibration();
	}

	public float GetValue()
	{
		float retval = 0f;

		if (IsAvailable)
			retval = m_portInput.GetValue("x", m_min, m_max, 0, 1);

		retval = 1f - Mathf.Clamp01(retval);

		return retval;
	}
}