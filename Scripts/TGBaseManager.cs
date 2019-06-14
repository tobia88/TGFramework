
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TGBaseManager: MonoBehaviour {
    protected TGController m_controller;

    public virtual void Init( TGController _controller ) {
        m_controller = _controller;
    }

    public virtual void ForceClose() { }
    public virtual IEnumerator StartRoutine() { yield return 1; }
    public virtual IEnumerator EndRoutine() { yield return 1; }
}