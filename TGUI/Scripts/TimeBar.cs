using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class TimeBar : MonoBehaviour
{
    public Image fillImage;

    private void Awake()
    {
        fillImage = transform.Find("Fill").GetComponent<Image>();
    }

    public void SetValue(float _value)
    {
        fillImage.fillAmount = _value;
    }
}
