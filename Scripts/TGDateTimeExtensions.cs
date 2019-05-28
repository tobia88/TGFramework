using System;

public static class TGDateTimeExtensions {
    public static string ToFileFormatString( this DateTime time ) {
        return time.ToString( "yyyy_MM_dd_HH_mm_ss" );
    }

    public static string ToDateString( this DateTime time ) {
        return time.ToString( "yyyy/M/d HH:mm:ss" );
    }
}