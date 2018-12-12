using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TGDXHeatmapPanel : TGDXBaseCentre {
	public RawImage heatmapImg;
	public GameObject warningTxt;
	public Text deviceTxt;

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
