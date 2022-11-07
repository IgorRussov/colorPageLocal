using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Camera camera;
    public float drawingPadding;
    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ViewRectWithCamera(Rect rect)
    { 
        float h = rect.height / PositionConverter.SvgPixelsPerUnit;
        float w = rect.width / PositionConverter.SvgPixelsPerUnit + drawingPadding;
        float ar = 1.0f * camera.pixelWidth / camera.pixelHeight;

        //camera.orthographicSize = w / ar;
        camera.orthographicSize = w*1.1f;
    }
}
