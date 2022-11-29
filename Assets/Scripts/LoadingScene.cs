using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [Header("Flowing paint")]
    public RectTransform flowingPaint;
    public float startBottom;
    public float endBottom;
    [Header("Progress bar")]
    public RectTransform progressBarFill;
    public RectTransform penIcon;
    public float endRight;
    public float startPenPosX;
    public float rotationAmplitude;
    public float rotationPeriod;

    private float startRight;
    private float penOffset;

    private void InitInterfaceValues()
    {
        startRight = endRight + progressBarFill.rect.width;
        penOffset = progressBarFill.rect.width;
    }

    private void Start()
    {
        InitInterfaceValues();
        LoadScene(1);
    }

    [Range(0, 1)]
    public float progressValue = 0;

    private void UpdatePaintFlow()
    {
        float newBottom = Mathf.Lerp(startBottom, endBottom, progressValue);
        flowingPaint.offsetMin = new Vector2(flowingPaint.offsetMin.x, newBottom);
    }

    private void UpdateProgressBar()
    {
        float currentRight = Mathf.Lerp(startRight, endRight, progressValue);
        progressBarFill.offsetMax = new Vector2(-currentRight, progressBarFill.offsetMax.y);
        penIcon.anchoredPosition = new Vector2(-currentRight + endRight + startPenPosX + penOffset, penIcon.anchoredPosition.y);
    }

    private void UpdatePenRotation()
    {
        float pos = penIcon.anchoredPosition.x;
        float phase = Mathf.Sin(pos / rotationPeriod);
        float value = phase * rotationAmplitude;

        penIcon.rotation = Quaternion.Euler(0, 0, value);
    }

    private void Update()
    {
        UpdatePaintFlow();
        UpdateProgressBar();
        UpdatePenRotation();
    }

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);

        while (!operation.isDone)
        {
            progressValue = Mathf.Clamp01(operation.progress * 0.99f);

            yield return null;
        }
    }
}


