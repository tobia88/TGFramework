using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TGDXInputDataColumn : MonoBehaviour {

	public TGDXKeyTestBar testBarPrefab;
	public TGDXKeyTestBar[] testBars;
	public Transform keyValueContainer;
	public Text titleTxt;
	public Text inputTxt;

	private KeyPortData m_keyportData;

	public void Init(KeyPortData _keyportData)
	{
		m_keyportData = _keyportData;

		titleTxt.text = _keyportData.name;

		SetupTestBars();
	}

	private void SetupTestBars()
	{
		int length = m_keyportData.value.Length;
		testBars = new TGDXKeyTestBar[length];

		for (int i = 0; i < length; i++)
		{
			var keyportValue = m_keyportData.value[i];

			var bar = CreateBar(keyportValue);

			testBars[i] = bar;	
		}
	}

	private TGDXKeyTestBar CreateBar(KeyPortValue value)
	{
		var bar = Instantiate<TGDXKeyTestBar>(testBarPrefab, keyValueContainer);
		return bar;
	}

	public void OnUpdate()
	{
		if (testBars == null)
			return;

		inputTxt.text = GetInputText();

		for (int i = 0; i < testBars.Length; i++)
		{
			testBars[i].UpdateData(m_keyportData.value[i]);
		}
	}

	private string GetInputText()
	{
		KeyPortInput[] input = m_keyportData.input;

		var retval = string.Empty;

		if (input == null || input.Length <= 0)
			return retval;

		retval += input[0].ToString();

		for (int i = 1; i < input.Length; i++)
		{
			retval += ", " + input[i].ToString();
		}

		return retval;
	}
}
