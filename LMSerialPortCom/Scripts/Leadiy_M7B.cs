using System;
using System.Collections;
using System.IO.Ports;
using System.Linq;
using System.Text;
using UnityEngine;

public class Leadiy_M7B : LMBasePortResolver
{
    private const string CODE_DETECTION = "A7 7A 72 ";
    private const string ACCELERATE_DETECTION = "A7 7A 71 ";
    private const string CODE_RESET_DIRE = "D66D303000";
    private const string CODE_WRITE_DIRE = "D66D3030";

    private string m_getString;

    private Vector3 m_angle;
    private Vector3 m_acceleration;

    public Vector3 Acceleration { get { return m_acceleration; } }
    public Vector3 Angles { get { return m_angle; } }

    private int m_length;

    public override void Start()
    {
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

        m_getString += LMUtility.ByteToStr(_bytes);

        var angleValues = GetByteValues(m_getString, CODE_DETECTION);
        var accelValues = GetByteValues(m_getString, ACCELERATE_DETECTION);

        if (angleValues == null || accelValues == null)
            return;

        UpdateAngles(angleValues);
        UpdateAcceleration(accelValues);

        m_getString = string.Empty;
    }

    private int[] GetByteValues(string value, string detection)
    {
        int keyIndex = value.IndexOf(detection);
        
        if (keyIndex == -1)
            return null;

        var sub = m_getString.Substring(keyIndex + detection.Length);

        var split = sub.Split(' ');

        if (split.Length == 0 || string.IsNullOrEmpty(split[0]))
            return null;

        m_length = Convert.ToInt16(split[0], 16);

        if (split.Length < m_length + 1)
            return null;

        var m_checksum = Convert.ToInt16(split[1], 16);

        var byteValues = ConvertHexStringToInt16(split);

        if (byteValues == null || !LMUtility.CheckSum(m_checksum, byteValues))
            return null;

        return byteValues;
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

    private void UpdateAcceleration(int[] byteValues)
    {
        m_acceleration.x = LMUtility.ConvertBitwiseToInt16(byteValues[1] << 8 | byteValues[0]);
        m_acceleration.y = LMUtility.ConvertBitwiseToInt16(byteValues[3] << 8 | byteValues[2]);
        m_acceleration.z = LMUtility.ConvertBitwiseToInt16(byteValues[5] << 8 | byteValues[4]);
    }

    private void UpdateAngles(int[] byteValues)
    {
        m_angle.x = LMUtility.ConvertBitwiseToInt16((byteValues[1] << 8) | byteValues[0]);
        m_angle.y = LMUtility.ConvertBitwiseToInt16((byteValues[3] << 8) | byteValues[2]);
        m_angle.z = LMUtility.ConvertBitwiseToInt16((byteValues[5] << 8) | byteValues[4]);

        m_angle *= 0.01f;

        if (values == null)
            return;

        try
        {
            if (values.Length > 0)values[0].SetValue(m_angle.x);
            if (values.Length > 1)values[1].SetValue(m_angle.y);
            if (values.Length > 2)values[2].SetValue(m_angle.z);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            Debug.Log("Values Length: " + values.Length);
        }
    }
}