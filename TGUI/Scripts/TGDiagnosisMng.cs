using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TGDiagnosisMng : MonoBehaviour
{
	public bool isActive;
	public TGInputDataColumn inputDataColumnPrefab;
	public TGInputDataColumn[] inputDataColumns;

	private TGController m_controller;

	public void OnInit(TGController _controller)
	{
		m_controller = _controller;
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

	private void SetupColumns()
	{
		var currentPort = m_controller.inputSetting.CurrentPortInput;

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
		inputDataColumns = new TGInputDataColumn[1];
		var currentPortData = _portUtility.currentPortData;

		if (currentPortData != null)
		{
			var column = Instantiate<TGInputDataColumn>(inputDataColumnPrefab, transform);
			column.Init(currentPortData);

			inputDataColumns[0] = column;
		}
	}

	public void Quit()
	{
		TGController.Instance.Quit();
	}

	public void Recalibration()
	{
		TGController.Instance.inputSetting.CurrentPortInput.Recalibration();
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
		
		foreach (TGInputDataColumn c in inputDataColumns)
		{
			c.OnUpdate();
		}
	}
}