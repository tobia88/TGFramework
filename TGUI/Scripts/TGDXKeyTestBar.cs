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
		minTxt.text = value.Min.ToString();
		maxTxt.text = value.Max.ToString();
		equationTxt.text = value.equation;
		valueTxt.text = "raw: " + value.RawValue.ToString("0.00") + ", cur: " + value.value.ToString("0.00");

		progress.minValue = value.Min;
		progress.maxValue = value.Max;	
		progress.value = (float)value.value;
	}
}