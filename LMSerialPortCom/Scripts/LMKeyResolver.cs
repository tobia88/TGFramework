using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using UnityEngine;
using System.Linq;
using System.IO;
using TG;

public class KeyResolveValue
{
    private float m_default;
    private float m_rawLastValue;
    private float m_rawValue;
    private bool m_isInit;

    public string key;
    public string equation;
    public float value;
    public float min;
    public float max;
    public bool isDegree;

    public void Init(float _origin = 0f)
    {
        m_default = min + (max - min) * _origin;

        value     = m_default;
    }

    public void Recalibration()
    {
        value = m_default;
    }

    public void SetValue(float newVal)
    {
        if (float.IsNaN(newVal))
            newVal = 0f;

        if (!m_isInit)
        {
            m_rawLastValue = m_rawValue = newVal;
            m_isInit = true;
        }
        else
        {
            m_rawLastValue = m_rawValue;
            m_rawValue = newVal;
        }

        if (isDegree)
        {
            value = TGUtility.PreventValueSkipping(value, m_rawLastValue, m_rawValue);
        }
        else
        {
            value += (m_rawValue - m_rawLastValue);
        }

    }

    public float GetValue()
    {
        if (min != max)
        {
            return (value - min) / (max - min);
        }

        return value;
    }
}

public class KeyResolveInput
{
    public string key;
    public int length;
    public float value;
    public float bias;

    public void SetValue(float _value)
    {
        value = _value;
    }

    public float GetValue()
    {
        return value + bias;
    }

    public override string ToString()
    {
        return key + ": " + value.ToString();
    }
}

public class LMKeyResolver : LMBasePortResolver
{
    private string m_getString;
    private TGExpressionParser m_solver;

    public KeyResolveValue[] values;
    public KeyResolveInput[] inputs;

    public bool Threshold
    {
        get
        {
            if (string.IsNullOrEmpty(PortData.thresholdEquation))
                return true;

            return ResolveEquation(PortData.thresholdEquation) > 0;
        }
    }

    public override void Init(KeyPortData keyPortData)
    {
        base.Init(keyPortData);

        m_solver = new TGExpressionParser();

        InitInputs(keyPortData.input);
        InitValues(keyPortData.value);
    }

    public void SetBiases(string bias)
    {
        string[] split = bias.Split(';');

        for (int i = 0; i < split.Length; i++)
        {
            string[] resolved = split[i].Split(':');

            for (int j = 0; j < inputs.Length; j++)
            {
                if (inputs[j].key == resolved[0])
                {
                    inputs[j].bias += float.Parse(resolved[1]);
                    continue;
                }
            }
        }
    }

    private void InitInputs(KeyPortInputData[] datas)
    {
        inputs = new KeyResolveInput[datas.Length];

        for (int i = 0; i < datas.Length; i++)
        {
            var newInput = new KeyResolveInput();

            newInput.key    = datas[i].key;
            newInput.length = datas[i].length;

            inputs[i] = newInput;
        }
    }

    private void InitValues(KeyPortValueData[] datas)
    {
        values = new KeyResolveValue[datas.Length];

        for (int i = 0; i < datas.Length; i++)
        {
            var newValue = new KeyResolveValue();

            newValue.key      = datas[i].key;
            newValue.isDegree = datas[i].isDegree;


            newValue.min      = TGUtility.GetValueFromINI(datas[i].min);
            newValue.max      = TGUtility.GetValueFromINI(datas[i].max);

            newValue.equation = datas[i].equation;

            newValue.Init(datas[i].origin);

            values[i] = newValue;
        }
    }

    public override void ResolveBytes(byte[] _bytes)
    {
        base.ResolveBytes(_bytes);

        if (m_bytes.Length == 0)
            return;

        m_bytes = LMUtility.RemoveSpacing(m_bytes);

        FilterIds();
    }


    protected void FilterIds()
    {
        try
        {
            m_getString += Encoding.UTF8.GetString(m_bytes);

            for (int i = 0; i < inputs.Length; i++)
            {
                KeyResolveInput tmpInfo = inputs[i];

                string fullId = tmpInfo.key + ":";

                int index = m_getString.IndexOf(fullId);

                if (index >= 0 && (index + fullId.Length + tmpInfo.length) < m_getString.Length)
                {
                    string v = m_getString.Substring(index + fullId.Length, tmpInfo.length);

                    if (v.Length != tmpInfo.length)
                    {
                        Debug.Log("Value Before Flush: " + v);
                        // string getting mess, just flush it
                        Flush();
                        break;
                    }
                    else
                    {
                        tmpInfo.SetValue(float.Parse(v));
                        m_getString = m_getString.Remove(index, fullId.Length + tmpInfo.length);
                    }
                }
            }

            for (int i = 0; i < values.Length; i++)
            {
                var resolve = ResolveEquation(values[i].equation);
                values[i].SetValue(resolve);
            }

            if (m_getString != string.Empty)
                m_getString = m_getString.Replace(";", string.Empty);
        }
        catch (Exception _ex)
        {
            Debug.LogWarning(_ex);
        }
    }

    private void Flush()
    {
        Debug.Log("Flush: " + m_getString);
        m_getString = string.Empty;
    }

    public override void Recalibration()
    {
        foreach (var v in values)
            v.Recalibration();
    }

    public override float GetValue(string key)
    {
        if (!Threshold)
            return 0f;

        return GetValueByKey(key).GetValue();
    }

    public float ResolveEquation(string equation)
    {
        string resolved = equation;

        foreach (var i in inputs)
        {
            if (resolved.IndexOf(i.key) >= 0)
            {
                string v = i.GetValue().ToString();

                if (string.IsNullOrEmpty(v))
                    v = "0";

                resolved = resolved.Replace(i.key, v);
            }
        }
        return (float)m_solver.EvaluateExpression(resolved).Value;
    }

    public KeyResolveValue GetValueByKey(string key)
    {
        KeyResolveValue retval = values.FirstOrDefault(v => v.key == key);

        if (retval != default(KeyResolveValue))
            return retval;

        throw new System.ArgumentNullException("Invalid Key: " + key);
    }
}
