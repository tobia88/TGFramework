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

	private LMBasePortResolver m_keyResolver;

	public void Init(LMBasePortResolver keyResolver)
	{
		m_keyResolver = keyResolver;

		titleTxt.text = TGController.Instance.inputSetting.DeviceName;

		SetupTestBars();
	}

	private void SetupTestBars()
	{
		if (m_keyResolver.values == null)
			return;

		int length = m_keyResolver.values.Length;
		testBars = new TGDXKeyTestBar[length];

		for (int i = 0; i < length; i++)
		{
			var keyportValue = m_keyResolver.values[i];

			var bar = CreateBar(keyportValue);

			testBars[i] = bar;	
		}
	}

	private TGDXKeyTestBar CreateBar(KeyResolveValue value)
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
			testBars[i].UpdateData(m_keyResolver.values[i]);
		}
	}

	private string GetInputText()
	{
		KeyResolveInput[] input = m_keyResolver.inputs;

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
