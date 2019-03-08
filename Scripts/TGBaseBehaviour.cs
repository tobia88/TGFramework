using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TGBaseBehaviour : MonoBehaviour
{
    protected TGController m_controller;
    public void Init(TGController _controller)
    {
        m_controller = _controller;
    }

    public abstract IEnumerator StartRoutine();
}
