using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoBarUI : MonoBehaviour
{
    public TMP_Text infoName;

    public TMP_Text currentValue;
    public TMP_Text maxCValue;
    public Slider valueBar;

    public TMP_Text minValue;
    public TMP_Text midValue;
    public TMP_Text maxValue;

    public void UpdateBar(string labelText, long value, long min, long max, bool normalized)
    {
        if (this.infoName != null)
            this.infoName.text = labelText;

        if (this.currentValue != null)
            this.currentValue.text = value.ToShortString();

        if (this.maxCValue != null)
            this.maxCValue.text = max.ToShortString();

        if (valueBar != null)
        {
            if (max != 0)
            {
                float ratio;
                if (normalized)
                    ratio = (float)((value - min) / (double)(max - min));
                else
                    ratio = (float)value / max;
                valueBar.value = Mathf.Clamp01(ratio);
            }
            else
            {
                valueBar.value = 0f;
            }
        }

        if (minValue != null)
            minValue.text = min.ToShortString();

        if (midValue != null)
        {
            long midpoint = min + (max - min) / 2;
            midValue.text = midpoint.ToShortString();
        }

        if (maxValue != null)
            maxValue.text = max.ToShortString();
    }

}
