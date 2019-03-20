using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

public class Leadiy_M7B : LMBasePortResolver
{
    private const string CODE_DETECTION = "A7 7A 72 ";
    private const string CODE_RESET_DIRE = "D66D303000";
    private const string CODE_WRITE_DIRE = "D66D3030";

    private string m_getString;

    private int m_checksum;
    private Vector3 m_angle;

    private int m_length;

    public int[] byteValues; 

    public override void Init(LMBasePortInput _portInput)
    {
        base.Init(_portInput);
        SetupEvaluateData(TGController.Instance.gameConfig.evalData);
    }

    public override void Recalibration()
    {
        foreach (var v in values)
            v.Recalibration();
    }

    public override void Close()
    {
        Write(CODE_RESET_DIRE);
    }

    public override void ResolveBytes(byte[] _bytes)
    {
        if (_bytes == null || _bytes.Length == 0)
            return;

        for (int i = 0; i < _bytes.Length; i++)
        {
            m_getString += _bytes[i].ToString("X").PadLeft(2, '0') + " ";
        }

        int keyIndex = m_getString.IndexOf(CODE_DETECTION);

        if (keyIndex != -1)
        {
            var sub = m_getString.Substring(keyIndex + CODE_DETECTION.Length);

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

    private void SetupEvaluateData(EvalData setupData)
    {
        var currentValue = values[(int)setupData.valueAxis];
        currentValue.reverse = setupData.reverse;
        WriteDire(setupData.dire);
    }

    private void WriteDire(int index)
    {
        string fullCode = CODE_WRITE_DIRE + "0" + index;

        Write(fullCode);
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
        m_angle.x = LMUtility.ConvertBitwiseToInt16((_byteValues[1] << 8) | _byteValues[0]);
        m_angle.y = LMUtility.ConvertBitwiseToInt16((_byteValues[3] << 8) | _byteValues[2]);
        m_angle.z = LMUtility.ConvertBitwiseToInt16((_byteValues[5] << 8) | _byteValues[4]);

        m_angle *= 0.01f;

        if (values == null)
            return;

        if (values.Length > 0) values[0].SetValue(m_angle.x);
        if (values.Length > 1) values[1].SetValue(m_angle.y);
        if (values.Length > 2) values[2].SetValue(m_angle.z);
    }
}