using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TGDXCentre : MonoBehaviour
{
	public bool isActive;
	public Text bugText;
	public TGDXInputDataColumn inputDataColumnPrefab;
	public TGDXInputDataColumn[] inputDataColumns;
	public GameObject debugGroup;

	private TGController m_controller;

	public void OnInit(TGController _controller)
	{
		m_controller = _controller;
		debugGroup.SetActive(false);
		gameObject.SetActive(false);
	}

	public void SetActive(bool _active)
	{
		isActive = _active;

		gameObject.SetActive(isActive);

		if (isActive)
			SetupColumns();
		else
			ClearColumns();
	}

	public void DebugText(string _text)
	{
		bugText.text = _text;
		debugGroup.SetActive(true);
	}

	private void SetupColumns()
	{
		var currentPort = m_controller.inputSetting.portInput;

		if (currentPort != null && currentPort.isPortActive)
		{
			if (currentPort is LMBasePortUtility)
			{
				SetupPortKeyTypesGraph(currentPort as LMBasePortUtility);
			}
		}
	}

	private void SetupPortKeyTypesGraph(LMBasePortUtility _portUtility)
	{
		inputDataColumns = new TGDXInputDataColumn[1];
		var currentPortData = _portUtility.currentPortData;

		if (currentPortData != null)
		{
			var column = Instantiate<TGDXInputDataColumn>(inputDataColumnPrefab, transform);
			column.Init(currentPortData);

			inputDataColumns[0] = column;
		}
	}

	public void Quit()
	{
		TGController.Instance.Quit();
	}

	private void ClearColumns()
	{
		if (inputDataColumns == null)
			return;

		foreach (var c in inputDataColumns)
			Destroy(c.gameObject);

		inputDataColumns = null;
	}

	public void OnUpdate()
	{
		if (!isActive || inputDataColumns == null)
			return;
		
		foreach (TGDXInputDataColumn c in inputDataColumns)
		{
			c.OnUpdate();
		}
	}
}