using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using TG;
using UnityEngine;

public class KeyResolveValue {
    private float m_default;
    private bool m_isInit;

    public string key;
    public string equation;
    public double value;
    public bool isDegree;
    public bool raw;
    public bool reverse;
    public float damp;

    public float Ratio { get; private set; }
    public float StartMin { get; private set; }
    public float StartMax { get; private set; }
    public float Min { get { return StartMin * Ratio; } }
    public float Max { get { return StartMax * Ratio; } }

    public double LastRawValue { get; private set; }
    public double RawValue { get; private set; }
    public double TargetValue { get; private set; }

    public KeyResolveValue( KeyPortValueData _data, bool _degree, bool _raw, bool _reverse, float _damp ) {
        key = _data.key;
        isDegree = _degree;

        raw = _raw;
        damp = _damp;

        StartMin = TGUtility.GetValueFromINI( _data.min );
        StartMax = TGUtility.GetValueFromINI( _data.max );

        equation = _data.equation;

        TargetValue = m_default = ( _data.origin == -1 ) ?
            0 : Min + ( Max - Min ) * _data.origin;

        Debug.Log( "Key: " + key + "初始值为" + TargetValue );

        Ratio = 1;
    }

    public void SetRatio( float _ratio ) {
        Ratio = _ratio;
    }

    public void Recalibration() {
        m_isInit = false;
        TargetValue = m_default;
    }


    public void SetValue( double newVal ) {
        if( double.IsNaN( newVal ) || double.IsInfinity( newVal )) {
            return;
        }

        if( !m_isInit ) {
            LastRawValue = RawValue = newVal;
            m_isInit = true;
            Debug.Log( "Key: " + key + "获得的最初值为" + newVal );
            return;
        } else {
            LastRawValue = RawValue;
            RawValue = newVal;
        }

        if( raw ) {
            TargetValue = RawValue;
        }
        else {
            if( isDegree ) {
                var delta = ( RawValue - LastRawValue ) ;
                TargetValue = TGUtility.PreventValueSkipping( TargetValue, LastRawValue, RawValue, reverse );
            } else {
                int sign = ( reverse ) ? -1 : 1;
                float dist = ( float )( RawValue - LastRawValue );
                TargetValue += dist * sign;
            }
        }

        if( damp > 0 )
            value = Mathf.Lerp( ( float )value, ( float )TargetValue, damp );
        else
            value = TargetValue;
    }

    public float GetValue() {
        if( Min != Max ) {
            return ( float )( value - Min ) / ( Max - Min );
        }

        return ( float )value;
    }
}

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

        string txt = TGGameConfig.GetValue( "校准", string.Empty );

        if( !string.IsNullOrEmpty( txt ) ) {
            SetBiases( txt );
        }
    }

    public override float GetValue( int index ) {
        Debug.Log( "Total: " + InputTotal + ", Total Gap: " + inputTotalGap );
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

            var splitBySemicolon = m_getString.Split( ';' );
            bool testGetValue = false;

            for( int i = splitBySemicolon.Length - 1; i >= 0; i-- ) {

                var tmpSplit = splitBySemicolon[i];

                for( int j = 0; j < inputs.Length; j++ ) {

                    var tmpInput = inputs[j];

                    if( tmpSplit.Contains( tmpInput.key ) ) {
                        var splitByColon = tmpSplit.Split( ':' );

                        if( splitByColon.Length < 2 )
                            continue;

                        var tmpValue = splitByColon[1];

                        if( tmpValue.Length != tmpInput.length )
                            continue;

                        tmpInput.SetValue( float.Parse( tmpValue ) );
                        testGetValue = true;
                    }
                }
            }

            if( testGetValue )
                m_getString = string.Empty;

            if( values == null )
                return;

            for( int i = 0; i < values.Length; i++ ) {
                if( values[i] == null )
                    continue;

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