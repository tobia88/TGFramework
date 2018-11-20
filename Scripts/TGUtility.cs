using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TG;
using UnityEngine;

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
            if (key.IndexOf('(') >= 0)
            {
                var solver = new TGExpressionParser();

                int start = key.IndexOf('(') + 1;
                int end = key.IndexOf(')');

                var sliced = key.Substring(start, end - start).Split(',');
                Debug.Log("Sliced string: " + sliced);

                for (int i = 0; i < sliced.Length; i++)
                {
                    string cutkey = sliced[i];
                    Debug.Log("Cut Key: " + cutkey);

                    key = key.Replace(cutkey, TGController.Instance.gameConfig.GetValue(cutkey, 0f).ToString());
                }
                Debug.Log("String Converted: " + key);

                return (float)solver.EvaluateExpression(key).Value;
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
    }

    public static KeyInputConfig ParseConfigFile(string _configFileName)
    {
        return TGController.Instance.fileWriter.ReadJSON<KeyInputConfig>(_configFileName);
    }

    public static double PreventValueSkipping(double cx, double lv, double nv, bool r)
    {
        double sign = (r) ? -1 : 1;

        double di = nv - lv;
        double rv = cx;

        double delta = di;

        if (di <= -180) //Warp around toward right
        {
            delta = (360 - lv) + nv;
        }
        else if (di >= 180) //Warp around toward left
        {
            delta = (nv - 360) - lv;
        }

        rv += delta * sign;

        if (rv >= 360)
        {
            rv %= 360;
        }
        else if (rv <= -360)
        {
            rv %= -360;
        }

        return rv;
    }

    public static float FloatRemap(float value, float remapMin, float remapMax, float min, float max)
    {
        float ratio = (value - min) / Mathf.Abs(max - min);
        return remapMin + ratio * (remapMax - remapMin);
    }
}