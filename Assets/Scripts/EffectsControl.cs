using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsControl : MonoBehaviour
{
    public RectTransform uiCanvasTransform;
    public List<Color> textStrokeColors;
    public List<string> textQuips;

    public GameObject textQuipPrefab;

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
        gameControl.StrokeShapeFinished += SpawnQuip;
    }

    public void SpawnQuip(Vector3 worldPos, float error)
    {
        string text = RandomTextQuip();
        Color color = RandomTextColor();

        GameObject quipParent = new GameObject("Quip parent");
        quipParent.AddComponent<RectTransform>();
        quipParent.transform.SetParent(uiCanvasTransform);
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(worldPos);
        Debug.Log("World pos: " + worldPos + ", viewport pos: " + ViewportPosition);
        float x = ViewportPosition.x * Camera.main.pixelWidth;
        float y = ViewportPosition.y * Camera.main.pixelHeight;

        quipParent.transform.position = new Vector2(x, y);

        GameObject quip = GameObject.Instantiate(textQuipPrefab, quipParent.transform);

        quip.GetComponent<TextQuip>().SetTextAndColor(text, color);
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
