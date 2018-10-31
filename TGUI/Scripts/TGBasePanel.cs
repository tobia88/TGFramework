using UnityEngine;

public class TGBasePanel : MonoBehaviour
{
    protected Animator m_animator;

    protected virtual void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public virtual void Show()
    {
		m_animator.SetBool("OnShow", true);
    }

	public void Exit()
	{
		m_animator.SetBool("OnShow", false);
	}
}