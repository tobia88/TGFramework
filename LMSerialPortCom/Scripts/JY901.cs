using System;
using System.IO.Ports;
using System.Linq;
using UnityEngine;

public class JY901 : LMBasePortResolver
{
    private string m_getString;
    // private Vector3 m_lastGyro, m_lastAcc, m_lastAngle;
    private Vector3 m_gyro, m_acc, m_angle;

    // public int[] byteValues;

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
        // m_outputEuler = Vector3.zero;
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

        int accIndex = m_getString.IndexOf("55 51 ");
        int gyroIndex = m_getString.IndexOf("55 52 ");
        int angleIndex = m_getString.IndexOf("55 53 ");

        if (accIndex < 0 || gyroIndex < 0 || angleIndex < 0)
            return;

        string[] accSplit = m_getString.Substring(accIndex).Split(' ');
        string[] gyroSplit = m_getString.Substring(gyroIndex).Split(' ');
        string[] angleSplit = m_getString.Substring(angleIndex).Split(' ');

        if (accSplit.Length < 11 || gyroSplit.Length < 11 || angleSplit.Length < 11)
            return;

        int[] accArray = HexToArray(accSplit);
        int[] gyroArray = HexToArray(gyroSplit);
        int[] angleArray = HexToArray(angleSplit);

        if (accArray == null || gyroArray == null || angleArray == null)
            return;

        SetupAcceleration(accSplit);
        SetupAngularVel(gyroSplit);
        SetupAngle(angleSplit);

        ConsoleProDebug.Watch("JY901", ToString());

        m_getString = string.Empty;
    }

    private bool SetupAngle(string[] _split)
    {
        if (_split.Length < 11)
            return false;

        int[] array = HexToArray(_split);

        if (array == null)
            return false;

        // m_lastAngle = m_angle;

        m_angle.x = LMUtility.ConvertBitwiseToInt16((array[3] << 8) | array[2]) / 32768f * 180f;
        m_angle.y = LMUtility.ConvertBitwiseToInt16((array[5] << 8) | array[4]) / 32768f * 180f;
        m_angle.z = LMUtility.ConvertBitwiseToInt16((array[7] << 8) | array[6]) / 32768f * 180f;

        return true;
    }

    private void SetupAcceleration(string[] _split)
    {
        int[] array = HexToArray(_split);

        if (array == null)
            return;

        float g = 9.8f;

        // m_lastAcc = m_acc;

        m_acc.x = LMUtility.ConvertBitwiseToInt16((array[3] << 8) | array[2]) / 32768f * 16 * g;
        m_acc.y = LMUtility.ConvertBitwiseToInt16((array[5] << 8) | array[4]) / 32768f * 16 * g;
        m_acc.z = LMUtility.ConvertBitwiseToInt16((array[7] << 8) | array[6]) / 32768f * 16 * g;
    }

    private void SetupAngularVel(string[] _split)
    {
        int[] array = HexToArray(_split);

        if (array == null)
            return;


        // m_lastGyro = m_gyro;
        
        m_gyro.x = LMUtility.ConvertBitwiseToInt16(((array[3] << 8) | array[2])) / 32768f * 2000;
        m_gyro.y = LMUtility.ConvertBitwiseToInt16(((array[5] << 8) | array[4])) / 32768f * 2000;
        m_gyro.z = LMUtility.ConvertBitwiseToInt16(((array[7] << 8) | array[6])) / 32768f * 2000;
    }

    private int[] HexToArray(string[] _split)
    {
        int[] retval = new int[10];

        for (int i = 0; i < retval.Length; i++)
        {
            if (string.IsNullOrEmpty(_split[i]))
                return null;

            retval[i] = Convert.ToInt32(_split[i], 16);
        }

        if (string.IsNullOrWhiteSpace(_split[10]))
            return null;

        int checkSumVal = Convert.ToInt32(_split[10], 16);

        if (!CheckSum(retval, checkSumVal))
            return null;

        return retval;

    }

    private bool CheckSum(int[] testValues, int checkSum)
    {
        // Debug.Log("Test Values: " + testValues.ToArrayString() + "," + checkSum);
        // Debug.Log("Test Sum: " + testValues.Sum() + ", check sum: " + checkSum);
        return true;
        // return checkSum == testValues.Sum();
    }

    public override string ToString()
    {
        string format = "Gyro: {0}\nAcc: {1}\nAngle:{2}";
        return string.Format(format, m_gyro, m_acc, m_angle);
    }
}