using UnityEngine;
using System.Collections.Generic;

public class LMTouchCtrl : MonoBehaviour
{
    private Dictionary<string, float> m_lastPosDict;

    public enum TouchDimension
    {
        Two,
        Three
    }

    public TouchDimension touchDimension;

    private void OnEnable()
    {
        m_lastPosDict = new Dictionary<string, float>();
    }

    public float GetValue(string key)
    {
        var cam = Camera.main;
        //TODO: Touch and Mouse Logical
        return 0f;
    }

    private void StorePos(string key, float value)
    {
        if (m_lastPosDict.ContainsKey(key))
            m_lastPosDict[key] = value;
        else
            m_lastPosDict.Add(key, value);
    }
}