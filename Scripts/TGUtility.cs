using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public static class TGUtility
{
	public static string ParseDateTimeToString(DateTime _dateTime)
	{
		return _dateTime.ToString("yyyy/M/d HH:mm:ss");
	}

    public static float GetValueFromINI(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return 0f;
        }
        else
        {
            float retval = 0f;
            if (float.TryParse(key, out retval))
            {
                return retval;
            }
            else
            {
                return TGController.Instance.gameConfig.GetValue(key, 0);
            }
        }
    }

    public static float PreventValueSkipping(float cx, float lv, float nv)
    {
        float d = nv - lv;
        float rv = cx;

        if (d < -180)
        {
            rv += nv + (360 - lv);
        }
        else if (d > 180)
        {
            rv -= nv - (360 - lv);
        }
        else
        {
            rv += d;
        }

        return rv;
    }

    public static float FloatRemap(float value, float remapMin, float remapMax, float min, float max)
    {
        float ratio = (value - min) / Mathf.Abs(max - min);
        return remapMin + ratio * (remapMax - remapMin);
    }
}
