Shader "Custom/CubemapBlend"
{
    Properties
    {
        _Cubemap1 ("Cubemap 1", Cube) = "" {}
        _Cubemap2 ("Cubemap 2", Cube) = "" {}
        _Blend ("Blend", Range(0, 1)) = 0
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

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : POSITION;
            };

            samplerCUBE _Cubemap1;
            samplerCUBE _Cubemap2;
            float _Blend;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 tex1 = texCUBE(_Cubemap1, i.pos.xyz);
                half4 tex2 = texCUBE(_Cubemap2, i.pos.xyz);
                return lerp(tex1, tex2, _Blend);
            }
            ENDCG
        }
    }
}
