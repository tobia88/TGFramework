using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using TG;
using UnityEngine;

public class KeyResolveValue
{
    private float m_default;
    private bool m_isInit;

    public string key;
    public string equation;
    public double value;
    public float min;
    public float max;
    public bool isDegree;
    public bool raw;
    public bool reverse;

    public double LastRawValue { get; private set; }
    public double RawValue { get; private set; }

    public KeyResolveValue(KeyPortValueData _data, bool _degree, bool _raw, bool _reverse)
    {
        key = _data.key;
        isDegree = _degree;

        raw = _raw;

        min = TGUtility.GetValueFromINI(_data.min);
        max = TGUtility.GetValueFromINI(_data.max);

        equation = _data.equation;

        value = m_default = (_data.origin == -1) ? 0 : min + (max - min) * _data.origin;
    }

    public void Recalibration()
    {
        value = m_default;
    }

    public void SetValue(double newVal)
    {
        if (double.IsNaN(newVal))
            newVal = 0f;

        if (!m_isInit)
        {
            LastRawValue = RawValue = newVal;
            m_isInit = true;
        }
        else
        {
            LastRawValue = RawValue;
            RawValue = newVal;
        }

        if (raw)
        {
            value = (float)newVal;
            return;
        }

        if (isDegree)
        {
            value = TGUtility.PreventValueSkipping(value, LastRawValue, RawValue, reverse);
        }
        else
        {
            int sign = (reverse) ? -1 : 1;
            float dist = (float)(RawValue - LastRawValue);
            value += dist * sign;
        }

    }

    public float GetValue()
    {
        if (min != max)
        {
            return (float)((value - min) / (max - min));
        }

        return (float)value;
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


    public bool Threshold
    {
        get
        {
            if (string.IsNullOrEmpty(PortData.thresholdEquation))
                return true;

            return ResolveEquation(PortData.thresholdEquation) > 0;
        }
    }

    public override void Init(LMBasePortInput _portInput)
    {
        base.Init(_portInput);

        m_solver = new TGExpressionParser();
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