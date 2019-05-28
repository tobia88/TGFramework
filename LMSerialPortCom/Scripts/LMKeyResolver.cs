using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using TG;
using UnityEngine;

// ?????????????keyInputConfig.json???????????
public class KeyResolveValue {
    private float m_default;
    private bool m_isInit;

    public string key;
    public string equation;
    public double value;
    public bool isDegree;
    public bool raw;
    public bool reverse;

    public float Ratio { get; private set; }
    public float StartMin { get; private set; }
    public float StartMax { get; private set; }
    public float Min { get { return StartMin * Ratio; } }
    public float Max { get { return StartMax * Ratio; } }

    public double LastRawValue { get; private set; }
    public double RawValue { get; private set; }

    public KeyResolveValue( KeyPortValueData _data, bool _degree, bool _raw, bool _reverse ) {
        key = _data.key;
        isDegree = _degree;

        raw = _raw;

        StartMin = TGUtility.GetValueFromINI( _data.min );
        StartMax = TGUtility.GetValueFromINI( _data.max );

        equation = _data.equation;

        value = m_default = ( _data.origin == -1 ) ?
            0 : Min + ( Max - Min ) * _data.origin;

        Ratio = 1;
    }

    public void SetRatio( float _ratio ) {
        Ratio = _ratio;
    }

    public void Recalibration() {
        value = m_default;
    }

    private int _delayFrame = 30;

    public void SetValue( double newVal ) {
        if( _delayFrame > 0 ) {
            _delayFrame--;
            return;
        }

        if( double.IsNaN( newVal ) )
            newVal = 0f;

        if( !m_isInit ) {
            LastRawValue = RawValue = newVal;
            m_isInit = true;
        } else {
            LastRawValue = RawValue;
            RawValue = newVal;
        }

        if( raw ) {
            value = ( float )RawValue;
            return;
        }

        if( isDegree ) {
            value = TGUtility.PreventValueSkipping( value, LastRawValue, RawValue, reverse );
        } else {
            int sign = ( reverse ) ? -1 : 1;
            float dist = ( float )( RawValue - LastRawValue );
            value += dist * sign;
        }

    }

    public float GetValue() {
        if( Min != Max ) {
            return ( float )( value - Min ) / ( Max - Min );
        }

        return ( float )value;
    }
}

// ??????????keyInputConfig.json?Key??
public class KeyResolveInput {
    public string key;
    public int length;
    public float value;
    public float bias;

    public void SetValue( float _value ) {
        value = _value;
    }

    public float GetValue() {
        return value - bias;
    }

    public override string ToString() {
        string retval = key + ": " + GetValue().ToString();

        if( bias != 0f )
            retval += "(" + value.ToString() + ( bias * -1f ).ToString() + ")";

        return retval;
    }
}

public class LMKeyResolver: LMBasePortResolver {
    private string m_getString;

    public int inputTotalGap;

    public float InputTotal {
        get {
            if( inputs == null )
                return 0;

            return inputs.Sum( i => i.GetValue() );
        }
    }

    public bool Threshold {
        get { return inputTotalGap == 0 || InputTotal >= inputTotalGap; }
    }

    public override void Init( LMBasePortInput _portInput ) {
        base.Init( _portInput );

        inputTotalGap = _portInput.KeyportData.inputTotalGap;

        string txt = TGController.Instance.gameConfig.GetValue( "??", string.Empty );

        if( !string.IsNullOrEmpty( txt ) ) {
            SetBiases( txt );
        }
    }

    public override float GetValue( int index ) {
        if( !Threshold ) {
            return 0f;
        }

        return base.GetValue( index );
    }

    public void SetBiases( string bias ) {
        string[] split = bias.Split( ';' );

        for( int i = 0; i < split.Length; i++ ) {
            string[] resolved = split[i].Split( ':' );

            for( int j = 0; j < inputs.Length; j++ ) {
                if( inputs[j].key == resolved[0] ) {
                    inputs[j].bias += float.Parse( resolved[1] );
                    continue;
                }
            }
        }
    }

    public override void ResolveBytes( byte[] _bytes ) {
        if( _bytes == null || _bytes.Length == 0 )
            return;

        _bytes = LMUtility.RemoveSpacing( _bytes );

        FilterIds( _bytes );
    }

    protected void FilterIds( byte[] _bytes ) {
        try {
            m_getString += Encoding.ASCII.GetString( _bytes );

            // ?AB:3000;AC:2030??????????
            // ???[AB:3000, AC:2030]
            var splitBySemicolon = m_getString.Split( ';' );

            // ?????????????????????????????????????
            bool testGetValue = false;

            for( int i = splitBySemicolon.Length - 1; i >= 0; i-- ) {

                var tmpSplit = splitBySemicolon[i];

                // ??keyInputConfig.json????Key???????
                for( int j = 0; j < inputs.Length; j++ ) {

                    var tmpInput = inputs[j];

                    if( tmpSplit.Contains( tmpInput.key ) ) {
                        // ?AB:3000???[AB, 3000]
                        var splitByColon = tmpSplit.Split( ':' );
                        var tmpValue = splitByColon[1];

                        // ???????????????????????
                        if( tmpValue.Length != tmpInput.length )
                            continue;

                        tmpInput.SetValue( float.Parse( tmpValue) );
                        testGetValue = true;
                    }
                }
            }

            // ?????????????????????
            if( testGetValue )
                m_getString = string.Empty;

            // ???????????
            for( int i = 0; i < values.Length; i++ ) {
                var resolve = ResolveEquation( values[i].equation );
                values[i].SetValue( resolve );
            }

        } catch( Exception _ex ) {
            Debug.LogWarning( _ex );
        }
    }

    private void Flush() {
        Debug.Log( "Flush: " + m_getString );
        m_getString = string.Empty;
    }

    public override void Recalibration() {
        foreach( var v in values )
            v.Recalibration();
    }

}