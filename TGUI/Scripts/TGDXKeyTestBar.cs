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
		minTxt.text = value.min.ToString();
		maxTxt.text = value.max.ToString();
		equationTxt.text = value.equation;
		valueTxt.text = "raw: " + value.RawValue + ", cur: " + value.value;

		progress.minValue = value.min;
		progress.maxValue = value.max;	
		progress.value = (float)value.value;
	}
}