Shader "Skybox/6 Sided Blended" {
    Properties {
        _BlendFactor ("Blend Factor", Range(0, 1)) = 0.5
        _Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        [Gamma] _Exposure ("Exposure", Range(0.0, 8.0)) = 1.0
        _Rotation ("Rotation", Range(0.0, 360.0)) = 0.0
        [NoScaleOffset] _FrontTex1 ("Front [+Z] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _BackTex1 ("Back [-Z] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _LeftTex1 ("Left [+X] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _RightTex1 ("Right [-X] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _UpTex1 ("Up [+Y] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _DownTex1 ("Down [-Y] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _FrontTex2 ("Front [+Z] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _BackTex2 ("Back [-Z] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _LeftTex2 ("Left [+X] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _RightTex2 ("Right [-X] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _UpTex2 ("Up [+Y] (HDR)", 2D) = "grey" { }
        [NoScaleOffset] _DownTex2 ("Down [-Y] (HDR)", 2D) = "grey" { }
    }
    SubShader {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Pass {
            Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            sampler2D _FrontTex1, _BackTex1, _LeftTex1, _RightTex1, _UpTex1, _DownTex1;
            sampler2D _FrontTex2, _BackTex2, _LeftTex2, _RightTex2, _UpTex2, _DownTex2;
            float4 _Tint;
            float _Exposure;
            float _Rotation;
            float _BlendFactor;

            v2f vert (appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 coords = i.texcoord;
                fixed4 color1, color2;

                if (coords.z > 0.5) color1 = tex2D(_FrontTex1, coords.xy);
                else if (coords.z < -0.5) color1 = tex2D(_BackTex1, coords.xy);
                else if (coords.x > 0.5) color1 = tex2D(_RightTex1, coords.xy);
                else if (coords.x < -0.5) color1 = tex2D(_LeftTex1, coords.xy);
                else if (coords.y > 0.5) color1 = tex2D(_UpTex1, coords.xy);
                else if (coords.y < -0.5) color1 = tex2D(_DownTex1, coords.xy);

                if (coords.z > 0.5) color2 = tex2D(_FrontTex2, coords.xy);
                else if (coords.z < -0.5) color2 = tex2D(_BackTex2, coords.xy);
                else if (coords.x > 0.5) color2 = tex2D(_RightTex2, coords.xy);
                else if (coords.x < -0.5) color2 = tex2D(_LeftTex2, coords.xy);
                else if (coords.y > 0.5) color2 = tex2D(_UpTex2, coords.xy);
                else if (coords.y < -0.5) color2 = tex2D(_DownTex2, coords.xy);

                fixed4 color = lerp(color1, color2, _BlendFactor);
                color.rgb = color.rgb * _Exposure + _Tint.rgb;
                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
