using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TouchInfo
{
	public int index;
	public Vector2 startPos;
	public Vector2 lastPos;
	public Vector2 delta;
	public Vector2 position;
	public TouchPhase phase;

	public TouchInfo(int _index, Touch _touch)
	{
		this.index = _index;
		this.startPos = this.position = this.lastPos = _touch.position;
		this.delta = Vector2.zero;
		this.phase = _touch.phase;
	}

	public void Update(Touch t)
	{
		this.lastPos = this.position;

		this.position = t.position;

		this.delta = this.position - this.lastPos;

		this.phase = t.phase;
	}
}

public class TGTouchManager : MonoBehaviour
{
	public Dictionary<int, TouchInfo> touchDict;
	public void OnStart()
	{
		touchDict = new Dictionary<int, TouchInfo>();
	}

	public void OnUpdate()
	{
		for (int i = 0; i < Input.touchCount; i++)
		{
			Touch t = Input.GetTouch(i);

			if (t.phase == TouchPhase.Ended)
			{
				if (touchDict.ContainsKey(i))
					touchDict.Remove(i);
			}
			else
			{
				if (touchDict.ContainsKey(i))
				{
					touchDict[i].Update(t);
				}
				else
				{
					var ti = new TouchInfo(i, t);
					touchDict.Add(i, ti);
				}
			}
		}
	}
}