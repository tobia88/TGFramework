using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TGDXHeatmapPanel : TGDXBaseCentre
{
	public RawImage heatmapImg;
	public GameObject warningTxt;
	public Text deviceTxt;

	private HeatmapInput m_heatmapInput;

	public override void OnInit(TGController _controller)
	{
		base.OnInit(_controller);
		m_heatmapInput = _controller.heatmapInput;
	}

	public override void SetActive(bool _active)
	{
		base.SetActive(_active);

		if (m_heatmapInput.isActiveAndEnabled)
		{
			m_heatmapInput.updateRuntime = _active;

			if (_active)
			{
				m_heatmapInput.ApplyHeatmap();
			}
		}
	}

	public void SetTexture(Texture2D _tex)
	{
		heatmapImg.gameObject.SetActive(true);
		heatmapImg.texture = _tex;

		warningTxt.SetActive(false);
	}

	public void ShowWarning(string _crtDevice)
	{
		heatmapImg.gameObject.SetActive(false);
		deviceTxt.text = string.Format(deviceTxt.text, _crtDevice);

		warningTxt.SetActive(true);
	}
}