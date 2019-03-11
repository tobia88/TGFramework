using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TGBaseBehaviour : MonoBehaviour
{
    protected TGController m_controller;
    protected TGGameConfig m_gameConfig { get { return m_controller.gameConfig; } }
    protected TGInputSetting m_inputSetting { get { return m_controller.inputSetting; } }
    protected TGMainGame m_mainGame { get { return m_controller.mainGame; } }

    public void Init(TGController _controller)
    {
        m_controller = _controller;
    }

    public abstract IEnumerator SetupRoutine();
}
