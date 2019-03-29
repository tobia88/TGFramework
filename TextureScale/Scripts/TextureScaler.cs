using UnityEngine;

public class TextureScaler
{
    public enum ImageFilterMode : int {
        Nearest  = 0,
        Bilinear = 1,
        Average  = 2
    }

    public static Texture2D ResizeTexture (Texture2D src, int tw, int th)
    {
        var result = new Texture2D(tw, th, src.format, false);

        for (int y = 0; y < result.height; y++)
        {
            for (int x = 0; x < result.width; x++)
            {
                Color nc = src.GetPixelBilinear((float) x / (float)result.width, (float) y / (float)result.height);
                result.SetPixel(x, y, nc);
            }
        }

        result.Apply();

        return result;
    }

    public static Texture2D ResizeTexture (Texture2D src, ImageFilterMode mode, int nw, int nh)
    {
        Color[] srcColors = src.GetPixels();

        int srcWidth = src.width;
        int srcHeight = src.height;

        Texture2D newTex = new Texture2D(nw, nh, TextureFormat.RGBA32, false);

        int length = nw * nh;

        Color[] tgColors = new Color[length];

        Vector2 pixelSize = new Vector2((float) srcWidth / nw, (float) srcHeight / nh);

        Vector2 center = new Vector2();

        for (int i = 0; i < length; i++)
        {
            float x = (float) i % nw;
            float y = Mathf.Floor((float)i / nw);

            center.x = (x / nw) * srcWidth;
            center.y = (y / nh) * srcHeight;

            // Nearest neighbour
            if (mode == ImageFilterMode.Nearest)
            {
                center.x = Mathf.Round(center.x);
                center.y = Mathf.Round(center.y);

                int index = (int)((center.y * srcWidth) + center.x);
                tgColors[i] = srcColors[index];
            }

            // Bilinear
            else if (mode == ImageFilterMode.Bilinear)
            {
                // Get Ratios
                float rx = center.x - Mathf.Floor(center.x);
                float ry = center.y - Mathf.Floor(center.y);

                // Get Pixel Index
                int indexTL = (int)((Mathf.Floor(center.y) * srcWidth) + Mathf.Floor(center.x));
                int indexTR = (int)((Mathf.Floor(center.y) * srcWidth) + Mathf.Ceil(center.x));
                int indexBL = (int)((Mathf.Ceil(center.y) * srcWidth) + Mathf.Floor(center.x));
                int indexBR = (int)((Mathf.Ceil(center.y) * srcWidth) + Mathf.Ceil(center.x));

                tgColors[i] = Color.Lerp(
                    Color.Lerp(srcColors[indexTL], srcColors[indexTR], rx),
                    Color.Lerp(srcColors[indexBL], srcColors[indexBR], rx),
                    ry
                );
            }
            else if (mode == ImageFilterMode.Average)
            {
                // Calculate grid around point
                int xFrom = (int)Mathf.Max(Mathf.Floor(center.x - (pixelSize.x * 0.5f)), 0);
                int xTo   = (int)Mathf.Max(Mathf.Ceil(center.x - (pixelSize.x * 0.5f)), srcWidth);
                int yFrom = (int)Mathf.Max(Mathf.Floor(center.y - (pixelSize.y * 0.5f)), 0);
                int yTo   = (int)Mathf.Max(Mathf.Ceil(center.y + (pixelSize.y * 0.5f)), srcHeight);

                // Loop and Accumulate
                Color cTemp = new Color();
                float xGridCount = 0;

                for (int iy = yFrom; iy < yTo; iy++)
                {
                    for (int ix = xFrom; ix < xTo; ix++)
                    {
                        cTemp += srcColors[(int)(((float)iy * srcWidth) + ix)];

                        xGridCount++;
                    }
                }

                // Average Color
                tgColors[i] = cTemp / (float)xGridCount;
            }
        }

        newTex.SetPixels(tgColors);
        newTex.Apply();

        return newTex;
    }
}