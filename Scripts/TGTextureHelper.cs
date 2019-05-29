using UnityEngine;
using System.Collections;

public static class TGTextureHelper {
    public const float FIX_HEIGHT_THRESHOLD = 1.39f;
    public const int FIX_WIDTH = 700;
    public const int FIX_HEIGHT = 590;
    public static IEnumerator SaveTexture( Texture2D _tex, string _name ) {
        float ratio = ( float )_tex.width / _tex.height;
        int width = 0, height = 0;

        // 如果比例大于阈值，则使用Fix width方案
        // 反之使用Fix Height方案
        if( ratio > FIX_HEIGHT_THRESHOLD ) {
            width = FIX_WIDTH;
            height = Mathf.RoundToInt( width / ratio );
        } else {
            height = FIX_HEIGHT;
            width = Mathf.RoundToInt( height * ratio );

            // 如果使用Fix Height方案后，宽度还是超标
            // 则再转位Fix Width方案
            if( width > FIX_WIDTH ) {
                width = FIX_WIDTH;
                height = Mathf.RoundToInt( width / ratio );
            }
        }

        Debug.Log( "Saved Texture Sizes: " + width + ", " + height );

        _tex = TextureScaler.ResizeTexture( _tex, width, height );

        byte[] bytes = _tex.EncodeToPNG();

        Debug.Log( "Write Texture: " + _name );

        // 写入文件
        TGController.Instance.fileWriter.Write( _name, bytes );

        // 写到Dicionary里，好之后写入ret.txt
        TGData.SaveScreenshot( _name );

        yield return null;
    }
}