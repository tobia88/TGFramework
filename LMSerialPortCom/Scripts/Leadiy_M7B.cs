using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

public class Leadiy_M7B : LMBasePortResolver
{
    private string m_getString;

    private int m_checksum;
    private Vector3 m_lastGyro, m_lastAcc, m_lastAngle;
    private Vector3 m_gyro, m_acc, m_angle;

    private int m_length;
    public const string FULL_VALUE_CODE = "D66D2020";

    public int[] byteValues;

    public override void Init(LMBasePortInput _portInput)
    {
        base.Init(_portInput);
        string hexCode = StringToHex(FULL_VALUE_CODE);
        Debug.Log("Converting Code: " + FULL_VALUE_CODE + " to " + hexCode + " and send to port");
        _portInput.Port.Write(hexCode);
    }

    private string StringToHex(string hexStr)
    {
        return String.Concat(hexStr.Select(x => Convert.ToInt32(x).ToString("X")));
    }

    public override float GetValue(string key)
    {
        Vector3 euler = Vector3.zero;
        euler.x = (values.Length > 0) ? values[0].GetValue() : 0;
        euler.y = (values.Length > 1) ? values[1].GetValue() : 0;
        euler.z = (values.Length > 2) ? values[2].GetValue() : 0;

        return euler.GetPosFromAxis(key);
    }

    public override void Recalibration()
    {
        foreach (var v in values)
            v.Recalibration();
    }

    public override void ResolveBytes(byte[] _bytes)
    {
        base.ResolveBytes(_bytes);

        if (m_bytes.Length == 0)
            return;

        for (int i = 0; i < m_bytes.Length; i++)
        {
            m_getString += m_bytes[i].ToString("X").PadLeft(2, '0') + " ";
        }

        // string detect = "A7 7A 60 ";
        string detect = "A7 7A 72 ";

        int keyIndex = m_getString.IndexOf(detect);

        if (keyIndex != -1)
        {
            var sub = m_getString.Substring(keyIndex + detect.Length);

            var split = sub.Split(' ');

            if (split.Length == 0 || string.IsNullOrEmpty(split[0]))
                return;

            m_length = Convert.ToInt16(split[0], 16);

            if (split.Length >= m_length + 1)
            {
                m_checksum = Convert.ToInt16(split[1], 16);
                byteValues = ConvertHexStringToInt16(split);

                // return if the values it not ready
                if (byteValues == null)
                    return;

                // Debug.Log(byteValues.ToArrayString());

                if (!CheckSum(m_checksum, byteValues))
                    return;

                SetupKeyValues(byteValues);
                m_getString = string.Empty;
            }
        }
    }

    private int[] ConvertHexStringToInt16(string[] _split)
    {
        int[] retval = new int[m_length - 1];

        // index 0 is the length of frame, we don't need it
        // so we have to offset 1 to the right
        for (int i = 0; i < retval.Length; i++)
        {
            string strToConvert = _split[i + 2];

            if (string.IsNullOrEmpty(strToConvert))
            {
                return null;
            }

            retval[i] = Convert.ToInt16(strToConvert, 16);
        }

        return retval;
    }

    private bool CheckSum(int _chechSum, int[] _byteValues)
    {
        if (_byteValues == null)
            return false;

        int compare = 0;

        for (int i = 0; i < _byteValues.Length; i++)
        {
            compare ^= _byteValues[i];
        }

        return m_checksum == compare;
    }

    private void SetupKeyValues(int[] _byteValues)
    {
        // m_gyro.x = LMUtility.ConvertBitwiseToInt16((_byteValues[1] << 8) | _byteValues[0]);
        // m_gyro.y = LMUtility.ConvertBitwiseToInt16((_byteValues[3] << 8) | _byteValues[2]);
        // m_gyro.z = LMUtility.ConvertBitwiseToInt16((_byteValues[5] << 8) | _byteValues[4]);

        // m_acc.x = LMUtility.ConvertBitwiseToInt16((_byteValues[7] << 8) | _byteValues[6]);
        // m_acc.y = LMUtility.ConvertBitwiseToInt16((_byteValues[9] << 8) | _byteValues[8]);
        // m_acc.z = LMUtility.ConvertBitwiseToInt16((_byteValues[11] << 8) | _byteValues[10]);

        // m_angle.x = LMUtility.ConvertBitwiseToInt16((_byteValues[13] << 8) | _byteValues[12]);
        // m_angle.y = LMUtility.ConvertBitwiseToInt16((_byteValues[15] << 8) | _byteValues[14]);
        // m_angle.z = LMUtility.ConvertBitwiseToInt16((_byteValues[17] << 8) | _byteValues[16]);

        m_angle.x = LMUtility.ConvertBitwiseToInt16((_byteValues[1] << 8) | _byteValues[0]);
        m_angle.y = LMUtility.ConvertBitwiseToInt16((_byteValues[3] << 8) | _byteValues[2]);
        m_angle.z = LMUtility.ConvertBitwiseToInt16((_byteValues[5] << 8) | _byteValues[4]);

        m_angle *= 0.01f;

        if (values.Length >= 0) values[0].SetValue(m_angle.x);
        if (values.Length >= 1) values[1].SetValue(m_angle.y);
        if (values.Length >= 2) values[2].SetValue(m_angle.z);
    }


    public override string ToString()
    {
        string format = "Gyro: {0}\nAcc: {1}\nAngle:{2}";
        return string.Format(format, m_gyro, m_acc, m_angle);
    }
}