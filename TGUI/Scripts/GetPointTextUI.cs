using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPointTextUI : MonoBehaviour
{
    public float offsetY;
    public float duration;

    private TMPro.TextMeshProUGUI m_textMesh;

    private void Awake()
    {
        m_textMesh = GetComponent<TMPro.TextMeshProUGUI>();
    }

    private void Start()
    {
        m_textMesh.DOFade(0f, duration / 2f).SetDelay(duration / 2);

        var targetY = transform.localPosition.y + offsetY;
        transform.DOLocalMoveY(targetY, duration).SetEase(Ease.OutQuart).OnComplete(() => Destroy(gameObject));
    }

    public void SetText(string _text)
    {
        m_textMesh.text = _text;
    }
}
