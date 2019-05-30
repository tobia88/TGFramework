using System;
using System.Collections.Generic;

public static class TGData {
    public static DateTime startTime;
    public static DateTime endTime;
    public static Dictionary<string, string> extraData = new Dictionary<string, string>();

    private const string SCORE_KEY = "??";
    private const string MAIN_SCREENSHOT_KEY = "??";

    public static void SaveScreenshot( string _fileName ) {
        var key = MAIN_SCREENSHOT_KEY;

        if( extraData.ContainsKey( key ) ) {
            extraData[key] += "|" + _fileName;
        } else {
            extraData.Add( key, _fileName );
        }
    }

    public static void SaveScore( int score ) {
        if( extraData.ContainsKey( SCORE_KEY ) )
            extraData[SCORE_KEY] = score.ToString();
        
        else
            extraData.Add( SCORE_KEY, score.ToString() );
    }
}