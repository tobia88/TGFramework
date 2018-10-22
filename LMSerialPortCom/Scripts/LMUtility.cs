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

    public static byte[] RemoveSpacing(byte[] _bytes)
    {
        int i = _bytes.Length - 1;

        while (_bytes[i] == 0 && i > 0)
            i--;

        byte[] retval = new byte[i+1];

        Array.Copy(_bytes, retval, i + 1);

        return retval;
    }
}
