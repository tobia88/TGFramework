using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCentre : MonoBehaviour
{
    protected TGGameScene m_baseScene;

    void Start()
    {
        m_baseScene = GetComponent<TGGameScene>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            WhilePressSpace();
        }
    }

    protected virtual void WhilePressSpace()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            m_baseScene.TimeLeft = 0;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            m_baseScene.Restart();
        }
    }
}
