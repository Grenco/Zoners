using UnityEngine;
using UnityEngine.UI;

public class HealthBarControl : MonoBehaviour
{
    public GameObject bar;
    public Color barColor;
    private float barHeight;
    private float barWidth;
    private float maxHeight;
    private float width;

    // Start is called before the first frame update
    private void Start()
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

    private void UpdateBar()
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