using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsControl : MonoBehaviour
{
    public RectTransform uiCanvasTransform;
    [Header("Text quip properties")]
    public List<Color> textStrokeColors;
    public List<string> textQuips;
    public GameObject textQuipPrefab;
    [Header("End line particles")]
    public GameObject endLineParticlesPrefab;


    private bool textQuipMustGoLeft;
    private float perfectStrokeMargin;

    private Color RandomTextColor()
    {
        return textStrokeColors[Random.Range(0, textStrokeColors.Count)];
    }

    private string RandomTextQuip()
    {
        return textQuips[Random.Range(0, textQuips.Count)];
    }

    public void BindToEvents(GameControl gameControl)
    {
        gameControl.StrokeShapeFinished += ShapeFinished;
        perfectStrokeMargin = gameControl.perfectStrokeMargin;
    }

    public void ShapeFinished(Vector3 pos, float error)
    {
        if (error < perfectStrokeMargin)
        {
            SpawnQuip(pos);
            SpawnEndLineParticles(pos);
        }
    }

    public void SpawnEndLineParticles(Vector3 worldPos)
    {
        GameObject newParticles = GameObject.Instantiate(endLineParticlesPrefab, transform);
        newParticles.transform.position = worldPos;

        float newScale = newParticles.transform.localScale.x;
        float orthSize = Camera.main.orthographicSize;
        newScale *= orthSize / 5;
        newParticles.transform.localScale = Vector3.one * newScale;
    }

    public void SpawnQuip(Vector3 worldPos)
    {
        string text = RandomTextQuip();
        Color color = RandomTextColor();

        GameObject quipParent = new GameObject("Quip parent");
        quipParent.AddComponent<RectTransform>();
        quipParent.transform.SetParent(uiCanvasTransform);
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(worldPos);
        //Debug.Log("World pos: " + worldPos + ", viewport pos: " + ViewportPosition);
        float x = ViewportPosition.x * Camera.main.pixelWidth;
        float y = ViewportPosition.y * Camera.main.pixelHeight;

        quipParent.transform.position = new Vector2(x, y);

        GameObject quip = GameObject.Instantiate(textQuipPrefab, quipParent.transform);

        quip.GetComponent<TextQuip>().SetProperties(text, color, !textQuipMustGoLeft);
        textQuipMustGoLeft = !textQuipMustGoLeft;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
