using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TGDXKeyTestBar : MonoBehaviour
{
	public Text nameTxt;
	public Text minTxt;
	public Text maxTxt;
	public Text valueTxt;
	public Text equationTxt;
	public Slider progress;

	public void UpdateData(KeyResolveValue value)
	{
		nameTxt.text = value.key + ":";

		string format = "{0}({1}*{2})";
		minTxt.text = string.Format(format, value.Min.ToString(), value.StartMin.ToString(), value.Ratio.ToString());
		maxTxt.text = string.Format(format, value.Max.ToString(), value.StartMax.ToString(), value.Ratio.ToString());
		equationTxt.text = value.equation;
		valueTxt.text = "raw: " + value.RawValue.ToString("0.00") + ", cur: " + value.value.ToString("0.00");

		progress.minValue = 0f;
		progress.maxValue = 1f;	
		progress.value = value.GetValue();
	}
}