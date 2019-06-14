using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGResultMng: TGBaseManager {
    public const string SAVE_FILENAME = "ret.txt";
    public bool isWrited = false;
    public override IEnumerator StartRoutine() {
        ForceWrite();
        isWrited = true;
        yield return 1;
    }

    public void ForceWrite() {
        string path = TGPaths.Root + SAVE_FILENAME;

        INIParser ini = new INIParser();

        ini.Open( path );

        ini.WriteValue( "ret", "名称", TGData.GameNameCn );
        ini.WriteValue( "ret", "种类", "2" );
        ini.WriteValue( "ret", "开始时间", TGData.startTime.ToDateString() );
        ini.WriteValue( "ret", "结束时间", TGData.endTime.ToDateString() );

        WriteExtraData( ini );

        ini.Close();

        Debug.Log( "Writing Finished" );
    }

    private void WriteExtraData( INIParser _ini ) {
        var dict = TGData.extraData;
        foreach( string k in dict.Keys ) {
            _ini.WriteValue( "ret", k, dict[k] );
        }
    }
}
