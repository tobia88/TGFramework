using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPointText : MonoBehaviour
{
    public float offsetY;
    public float duration;

    private TMPro.TextMeshPro m_textMesh;

    private void Awake()
    {
        m_textMesh = GetComponent<TMPro.TextMeshPro>();
    }

    private void Start()
    {
        m_textMesh.DOFade(0f, duration / 2f).SetDelay(duration / 2);
        transform.DOLocalMoveY(offsetY, duration).SetEase(Ease.OutQuart).OnComplete(() => Destroy(gameObject));
    }
}
