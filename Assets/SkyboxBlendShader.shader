Shader "Custom/BlendedSkybox"
{
    Properties
    {
        _Skybox1 ("Skybox 1", Cube) = "" {}
        _Skybox2 ("Skybox 2", Cube) = "" {}
        _BlendFactor ("Blend Factor", Range(0, 1)) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Background" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _Skybox1;
            samplerCUBE _Skybox2;
            float _BlendFactor;

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 color1 = texCUBE(_Skybox1, i.texcoord);
                half4 color2 = texCUBE(_Skybox2, i.texcoord);
                return lerp(color1, color2, _BlendFactor);
            }
            ENDCG
        }
    }
    FallBack "RenderType"
}