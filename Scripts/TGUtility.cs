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
        ConsoleProDebug.Watch("value", value.ToString());
        ConsoleProDebug.Watch("remap min", remapMin.ToString());
        ConsoleProDebug.Watch("remap max", remapMax.ToString());
        ConsoleProDebug.Watch("min", min.ToString());
        ConsoleProDebug.Watch("max", max.ToString());
        float ratio = (value - min) / Mathf.Abs(max - min);
        return remapMin + ratio * (remapMax - remapMin);
    }
}
