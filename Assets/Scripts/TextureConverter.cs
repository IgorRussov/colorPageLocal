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

}
