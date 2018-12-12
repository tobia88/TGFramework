using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TGDXBaseCentre : MonoBehaviour {

	protected TGController m_controller;
	public bool isActive { get; protected set; }

	public virtual void OnInit(TGController _controller)
	{
		m_controller = _controller;
		gameObject.SetActive(false);
	}

	public virtual void SetActive(bool _active)
	{
		isActive = _active;

		gameObject.SetActive(isActive);
	}
}
