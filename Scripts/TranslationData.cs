﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ValueAxis
{
	xyz,
	yzx,
	zxy
}

[System.Serializable]
public struct EvaluationSetupData
{
	public string cnTitle;
	public string animName;
	public string FilterAnimName
	{
		get
		{
			string[] retval = animName.Split('_');
			return retval[0];
		}
	}
	public ValueAxis valueAxis;
	public bool reverse;
	public bool isFullAxis;
	public int dire;

	public override string ToString()
	{
		return string.Format("体侧: {0}, Axis: {1}, Full Axis: {2}", cnTitle, valueAxis, isFullAxis);
	}
}

[CreateAssetMenu(menuName="Data/Translation Data")]
public class TranslationData : ScriptableObject
{
	public EvaluationSetupData[] infos;

	public EvaluationSetupData GetInfo(string _chineseName)
	{
		foreach(var info in infos)
		{
			if (info.cnTitle == _chineseName)
				return info;
		}

		throw new System.NullReferenceException("Missing setup: " + _chineseName);
	}
}
