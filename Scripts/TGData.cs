using System;
using System.Collections.Generic;

public static class TGData {
    public static DateTime startTime;
    public static DateTime endTime;
    public static Dictionary<string, string> extraData;

    private const string MAIN_SCREENSHOT_KEY = "图片";

    public static void SaveScreenshot( string _fileName ) {
        var key = MAIN_SCREENSHOT_KEY;

        if( extraData.ContainsKey( key ) ) {
            extraData[key] += "|" + _fileName;
        } else {
            extraData.Add( key, _fileName );
        }
    }
}