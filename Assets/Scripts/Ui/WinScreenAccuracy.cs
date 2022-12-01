using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScreenAccuracy : MonoBehaviour
{
    public TMPro.TMP_Text accuracyBarText;
    public float maxFillWidth;
    public RectTransform accuracyBarFillImage;
    public Animator accuracyAnimator;

    private bool updateAccuracy;
    private float maxAccuracy;

    private float startAccuracyPercent = 0.3f;
    private float accuracyTime = 1.5f;

    private float startAccuracyTime;

    // Update is called once per frame
    void Update()
    {
        if (updateAccuracy)
            UpdateAccuracy();
    }

    void StartAccuracyDisplay()
    {
        startAccuracyTime = Time.time;
        updateAccuracy = true;
    }

    void UpdateAccuracy()
    {
        float timePassed = Time.time - startAccuracyTime;
        if (timePassed > accuracyTime)
        {
            float currentAccuracy = Mathf.Lerp(maxAccuracy * startAccuracyPercent, maxAccuracy, timePassed / accuracyTime);
            if (Mathf.Ceil(currentAccuracy * 100) / 100 == 1)
                accuracyAnimator.SetTrigger("Accuracy100");
            updateAccuracy = false;
        }
        else
        {
            float currentAccuracy = Mathf.Lerp(maxAccuracy * startAccuracyPercent, maxAccuracy, timePassed / accuracyTime);
            currentAccuracy = Mathf.Ceil(currentAccuracy * 100) / 100;
            accuracyBarText.text = currentAccuracy.ToString("0%");
            float barPixels = maxFillWidth * currentAccuracy;
            accuracyBarFillImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barPixels);
        }
    }

    public void ShowAccuracyDisplay(float accuracy)
    {
        maxAccuracy = accuracy;
    }

    public void Disappear()
    {
        accuracyAnimator.SetTrigger("Disappear");
    }
}
