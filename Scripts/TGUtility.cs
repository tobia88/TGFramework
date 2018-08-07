using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TGUtility
{
	public static string ParseDateTimeToString(DateTime _dateTime)
	{
		return _dateTime.ToString("yyyy/M/d HH:mm:ss");
	}
}
