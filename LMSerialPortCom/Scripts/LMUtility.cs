using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LMUtility
{
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
