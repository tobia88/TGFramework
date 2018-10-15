using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TGKeyTestBar : MonoBehaviour
{
	public Text nameTxt;
	public Text minTxt;
	public Text maxTxt;
	public Text valueTxt;
	public Text equationTxt;
	public Slider progress;

	public void UpdateData(KeyPortValue value)
	{
		nameTxt.text = value.key + ":";
		minTxt.text = value.Min.ToString();
		maxTxt.text = value.Max.ToString();
		equationTxt.text = value.equation;
		valueTxt.text = value.value.ToString();

		progress.minValue = value.Min;
		progress.maxValue = value.Max;	
		progress.value = value.value;
	}
}