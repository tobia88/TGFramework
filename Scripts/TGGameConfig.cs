using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class EvalDataGroup {
    public EvalData[] infos;
}

public class TGGameConfig: TGBaseBehaviour {
    public string fileName;
    public string sectionName;
    public string evaluationFileName;
    public EvalData evalData;

    private INIParser m_iniParser;

    public override IEnumerator StartRoutine() {
        InitParser();

        if( !string.IsNullOrEmpty( evaluationFileName ) ) {
            var group = m_controller.fileWriter.ReadJSON<EvalDataGroup>( evaluationFileName );

            if( group != null ) {
                string cnTitle = GetValue( "体侧", string.Empty );
                evalData = GetConfigDataFromTitle( group, cnTitle );

            }
        } else {
            Debug.LogWarning( evaluationFileName + "Has not found" );
        }
        m_controller.ProgressValue += 0.1f;
        yield return new WaitForSeconds( 1f );
    }

    public override void ForceClose() {
        Close();
    }

    public override IEnumerator EndRoutine() {
        Close();
        yield return 1;
    }

    private EvalData GetConfigDataFromTitle( EvalDataGroup group, string cnTitle ) {
        return group.infos.FirstOrDefault( d => d.cnTitle == cnTitle );
    }

    public string GetValue( string key, string defaultValue ) {
        return m_iniParser.ReadValue( sectionName, key, defaultValue );
    }

    public int GetValue( string key, int defaultValue ) {
        return m_iniParser.ReadValue( sectionName, key, defaultValue );
    }

    public float GetValue( string key, float defaultValue ) {
        return ( float )m_iniParser.ReadValue( sectionName, key, defaultValue );
    }

    public void Close() {
        if( m_iniParser != null )
            m_iniParser.Close();
    }

    private void InitParser() {
        string path = m_controller.RootPath + "/" + fileName;

        m_iniParser = new INIParser();
        m_iniParser.Open( path );

        if( string.IsNullOrEmpty( m_iniParser.iniString ) ) {
            m_controller.ErrorQuit( "Config is NULL! Path = " + path );
            return;
        }
    }
}