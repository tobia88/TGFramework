using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatmapInput : MonoBehaviour
{
	public Texture2D plotTex;
	public Texture2D outputTex;
	public float plotSize = 1f;
	[Range(0f, 1f)]
	public float plotStrength = 0.5f;
	public Gradient gradient;
	public int drawInterval = 30;
	private float[] m_maskValues;
	private int m_width;
	private int m_height;
	private int m_count;

	public void Init(int _width, int _height)
	{
		m_width = _width;
		m_height = _height;

		outputTex = new Texture2D(m_width, m_height, TextureFormat.RGB24, false);

		for (int y = 0; y < outputTex.height; y++)
		{
			for (int x = 0; x < outputTex.width; x++)
			{
				outputTex.SetPixel(x, y, Color.black);
			}
		}

		outputTex.Apply();

		m_maskValues = new float[m_width * m_height];
	}

	public void DrawViewport(Vector2 vp)
	{
		if (!CheckDrawable())
			return;

		DrawPlot(vp.x * m_width, vp.y * m_height);
	}

	public void DrawPos(Vector2 pos)
	{
		if (!CheckDrawable())
			return;

		DrawPlot(pos.x, pos.y);
	}


	public bool CheckDrawable()
	{
		m_count++;
		if (m_count < drawInterval)
			return false;

		m_count -= drawInterval;
		return true;
	}


	private void DrawPlot(float _inputX, float _inputY)
	{
		int sizeInRatio = Mathf.RoundToInt(plotTex.width * plotSize);

		int inputX = Mathf.FloorToInt(_inputX);
		int inputY = Mathf.FloorToInt(_inputY);
		int plotHW = Mathf.RoundToInt(plotSize * plotTex.width * 0.5f);
		int plotHH = Mathf.RoundToInt(plotSize * plotTex.height * 0.5f);

		for (int y = 0; y < sizeInRatio; y++)
		{
			for (int x = 0; x < sizeInRatio; x++)
			{
				int bpx = inputX - plotHW + x;
				int bpy = inputY - plotHH + y;

				int pixelIndex = bpx + bpy * m_width;

				if (pixelIndex < 0 || pixelIndex > m_maskValues.Length)
					continue;

				float v = m_maskValues[pixelIndex] + plotTex.GetPixelBilinear((float)x / sizeInRatio, (float)y / sizeInRatio).r * plotStrength;

				if (v <= 0)
					continue;

				v = Mathf.Clamp01(v);

				m_maskValues[pixelIndex] = v;
				outputTex.SetPixel(bpx, bpy, gradient.Evaluate(v));
			}
		}

		outputTex.Apply();

	}
}