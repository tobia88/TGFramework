using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System;

public static class LMFileWriter {
    public static string Write( string _filePath, string _content ) {
        EnsureFolder( _filePath );

        try {
            using( FileStream fs = new FileStream( _filePath, FileMode.OpenOrCreate, FileAccess.Write ) ) {
                fs.SetLength( 0 );
                byte[] bytes = Encoding.UTF8.GetBytes( _content );
                fs.Write( bytes, 0, bytes.Length );
            }
        } catch( Exception _ex ) {
            return _ex.ToString();
        }

        return string.Empty;
    }

    public static string Write( string _filePath, byte[] _contents ) {
        try {
            using( FileStream fs = new FileStream( _filePath, FileMode.OpenOrCreate, FileAccess.Write ) ) {
                fs.Write( _contents, 0, _contents.Length );
            }
        } catch( Exception _ex ) {
            return _ex.ToString();
        }

        return string.Empty;
    }

    public static string Read( string _path, System.Action<List<string>> _callback ) {
        List<string> retval = new List<string>();

        try {
            using( StreamReader sr = new StreamReader( _path, Encoding.Default ) ) {
                string line = sr.ReadLine();

                while( !string.IsNullOrEmpty( line ) ) {
                    retval.Add( line );
                    line = sr.ReadLine();
                }
            }

            _callback( retval );
        } catch( Exception _ex ) {
            return _ex.ToString();
        }

        return string.Empty;
    }

    public static string Read( string _path ) {
        if( File.Exists( _path ) ) {
            return File.ReadAllText( _path );
        }

        return string.Empty;
    }

    public static T ReadJSON<T>( string _fileName ) {
        string data = Read( _fileName );

        if( data != string.Empty ) {
            return JsonUtility.FromJson<T>( data );
        }

        Debug.LogWarning( "Find no config file, perhaps you passed the wrong path" );
        return default( T );
    }

    private static void EnsureFolder( string _path ) {
        // 检测路径是否在独立的文件夹内
        int lastSlashIndex = _path.LastIndexOf( '/' );

        // 如果不在文件夹以内，则不进行任何操作
        if( lastSlashIndex < 0 )
            return;

        // 获取文件夹地路径，并创立文件夹
        string tmpPath = _path.Substring( 0, lastSlashIndex );

        if( !Directory.Exists( tmpPath ) ) {
            Debug.Log( "Folder Created: " + tmpPath );
            Directory.CreateDirectory( tmpPath );
        }
    }
}
