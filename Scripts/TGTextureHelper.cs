using UnityEngine;
using System.Collections;

public static class TGTextureHelper {
    public static IEnumerator SaveTexture( Texture2D _tex, string _name ) {
        float ratio = ( float )_tex.width / _tex.height;
        int width = 0, height = 0;

        if( ratio > 1.39f ) {
            width = 700;
            height = Mathf.RoundToInt( width / ratio );
        } else {
            height = 590;
            width = Mathf.RoundToInt( height * ratio );

            if( width > 700 ) {
                width = 700;
                height = Mathf.RoundToInt( width / ratio );
            }
        }

        Debug.Log( "Saved Texture Sizes: " + width + ", " + height );

        _tex = TextureScaler.ResizeTexture( _tex, width, height );

        byte[] bytes = _tex.EncodeToPNG();
        GameObject.Destroy( _tex );

        Debug.Log( "Write Texture: " + _name );

        TGController.Instance.fileWriter.Write( _name, bytes );

        // 写到Dicionary以写入ret.txt
        TGData.SaveScreenshot( _name );

        yield return null;
    }
}