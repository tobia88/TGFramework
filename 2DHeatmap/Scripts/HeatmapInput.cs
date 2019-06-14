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
	public bool updateRuntime = false;
	private float[] m_maskValues;
	private int m_width;
	private int m_height;
	private int m_count;
	private bool m_isInit;

	public void Init(int _width, int _height)
	{
		m_isInit = true;

		m_width = _width;
		m_height = _height;
		outputTex = new Texture2D(m_width, m_height, TextureFormat.RGB24, false);
		outputTex.wrapMode = TextureWrapMode.Clamp;


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

	public void DrawPos(Vector2 pos, float value = -1f)
	{
		if (!CheckDrawable())
			return;

		DrawPlot(pos.x, pos.y, value);
	}

	public bool CheckDrawable()
	{
		m_count++;
		if (m_count < drawInterval)
			return false;

		m_count -= drawInterval;
		return true;
	}

	public void ApplyHeatmap()
	{
		for (int y = 0; y < m_height; y++)
		{
			for (int x = 0; x < m_width; x++)
			{
				float v = m_maskValues[x + y * m_width];

				if (v == 0f)
					continue;

				outputTex.SetPixel(x, y, gradient.Evaluate(v));
			}
		}
		outputTex.Apply();
	}

	private void DrawPlot(float _inputX, float _inputY, float value = -1f)
	{
		if( !m_isInit ){
			Debug.LogWarning( "热图还没有初始化" );
			return;
		}

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

				if (pixelIndex < 0 || pixelIndex >= m_maskValues.Length)
					continue;

				float v = m_maskValues[pixelIndex] + plotTex.GetPixelBilinear((float)x / sizeInRatio, (float)y / sizeInRatio).r * plotStrength;

				if (value != -1f)
				{
					float nv = plotTex.GetPixelBilinear((float)x / sizeInRatio, (float)y / sizeInRatio).r * value * plotStrength;
					v = Mathf.Max(nv, v);
				}

				if (v <= 0)
					continue;

				v = Mathf.Clamp01(v);

				m_maskValues[pixelIndex] = v;

				if (updateRuntime)
					outputTex.SetPixel(bpx, bpy, gradient.Evaluate(v));
			}
		}

		if (updateRuntime)
			outputTex.Apply();
	}
}