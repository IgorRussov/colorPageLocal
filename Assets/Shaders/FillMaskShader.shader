Shader "Unlit/FillMaskShader"
{
    Properties{
       _MainTex("Base (RGB)", 2D) = "white" {}
       _Alpha("Alpha (A)", 2D) = "white" {}
    }

        SubShader{
            Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}

            ZWrite Off

            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB

            CGPROGRAM
            #pragma surface surf Lambert

            fixed4 _Color;
            sampler2D _MainTex;
            sampler2D _Alpha;

            struct Input {
              float2 uv_MainTex;
            };

            void surf(Input IN, inout SurfaceOutput o) {
              
              o.Albedo = tex2D(_MainTex, IN.uv_MainTex);
              o.Alpha = tex2D(_MainTex, IN.uv_MainTex).r > 0.2;
        }
        ENDCG

        
        }
        Fallback "Transparent/VertexUnlit"
}
