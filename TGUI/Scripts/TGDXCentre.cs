using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TGDXCentre : TGDXBaseCentre
{
	public string debugTxt { get; private set; }
	public Text bugText;
	public TGDXInputDataColumn inputDataColumnPrefab;
	public TGDXInputDataColumn[] inputDataColumns;
	public GameObject debugGroup;


	public override void OnInit(TGController _controller)
	{
		base.OnInit(_controller);
		debugGroup.SetActive(false);
	}

	public override void SetActive(bool _active)
	{
		base.SetActive(_active);

		if (_active)
			SetupColumns();
		else
			ClearColumns();

		if (!string.IsNullOrWhiteSpace(bugText.text))
		{
			debugGroup.SetActive(inputDataColumns == null || inputDataColumns.Length == 0);
		}
	}

	public void DebugText(string _text)
	{
		debugTxt = _text;
		bugText.text = debugTxt;
	}

	private void SetupColumns()
	{
		var currentPort = TGInputSetting.PortInput;

		if (currentPort != null && currentPort.IsPortActive)
		{
			SetupPortKeyTypesGraph(currentPort.CurrentResolver);
		}
	}

	private void SetupPortKeyTypesGraph(LMBasePortResolver _keyResolver)
	{
		inputDataColumns = new TGDXInputDataColumn[1];

		var column = Instantiate<TGDXInputDataColumn>(inputDataColumnPrefab, transform);
		column.Init(_keyResolver);

		inputDataColumns[0] = column;
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