using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour
{
    public GameObject bar;
    public Color barColor;
    float barHeight;
    float barWidth;
    float maxHeight;
    float width;

    // Start is called before the first frame update
    void Start()
    {
        maxHeight = gameObject.GetComponent<RectTransform>().rect.height;
        barWidth = gameObject.GetComponent<RectTransform>().rect.height - 2;
        barHeight = 0;
        UpdateBar();
    }

    public void SetBarFill(float fill)
    {
        barHeight = fill * maxHeight;
        UpdateBar();
    }

    void UpdateBar()
    {
        //bar.transform.lossyScale.Set(barWidth, barHeight, 0);
        bar.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, barHeight);
        bar.transform.localPosition = new Vector3(0, barHeight / 2 - maxHeight / 2 + 1);
    }

    public void SetBarColor(Color barColor)
    {
        bar.GetComponent<Image>().color = barColor;
    }
}
