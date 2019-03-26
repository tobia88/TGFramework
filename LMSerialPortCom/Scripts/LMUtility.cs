using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LMUtility
{
    public static float GetPosFromAxis(this Vector3 pos, string key)
    {
        if (key == "x")
            return pos.x;

        else if (key == "y")
            return pos.y;
            
        else if (key == "z")
            return pos.z;

        throw new System.ArgumentException("Key is only accept x, y or z");
    }

    public static Vector3 Reorder(this Vector3 output, string order)
    {
        switch(order)
        {
            case "yzx": return new Vector3(output.y, output.z, output.x);
            case "yxz": return new Vector3(output.y, output.x, output.z);
            case "xzy": return new Vector3(output.x, output.z, output.y);
            case "zxy": return new Vector3(output.z, output.x, output.y);
            case "zyx": return new Vector3(output.z, output.y, output.x);
            default   : return output;
        }
    }
     
    public static int ConvertBitwiseToInt16(int value)
    {
        UInt16 ui = Convert.ToUInt16(value);
        return (Int16)ui;
    }

    public static byte[] RemoveSpacing(byte[] _bytes)
    {
        int i = _bytes.Length - 1;

        while (_bytes[i] == 0 && i > 0)
            i--;

        byte[] retval = new byte[i+1];

        Array.Copy(_bytes, retval, i + 1);

        return retval;
    }

    public static string ByteToStr(byte[] _bytes)
    {
        string retval = string.Empty;
        for (int i = 0; i < _bytes.Length; i++)
        {
            retval += _bytes[i].ToString("X").PadLeft(2, '0') + " ";
        }
        return retval;
    }
}
