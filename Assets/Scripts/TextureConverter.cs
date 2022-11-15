using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureConverter : MonoBehaviour
{
    public ComputeShader textureConvertShader;
    public static TextureConverter Instance;

    private int offsetId;
    private int resultId;
    private int textureInId;
    private int sourceTextureSizeId;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        offsetId = Shader.PropertyToID("_Offset");
        resultId = Shader.PropertyToID("Result");
        textureInId = Shader.PropertyToID("_MainTex");
        sourceTextureSizeId = Shader.PropertyToID("_SourceTextureSize");
    }

    public RenderTexture PanTexture(Texture2D sourceTexture, Vector2 offset)
    {
        textureConvertShader.SetFloats(offsetId, new float[] { offset.x, offset.y });
        textureConvertShader.SetTexture(0, textureInId, sourceTexture);

        RenderTexture ret = ShapeUtils.CreateSceneSizedRenderTexture(Vector2.one * 100);

        int height = sourceTexture.height;
        int width = sourceTexture.width;
        textureConvertShader.SetFloats(sourceTextureSizeId, new float[] { width, height });

        textureConvertShader.SetTexture(0, resultId, ret);
        textureConvertShader.Dispatch(0, ret.width / 32 + 1, ret.height / 32 + 1, 1);
        return ret;
    }
}
