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