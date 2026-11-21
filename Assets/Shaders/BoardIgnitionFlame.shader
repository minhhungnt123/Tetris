Shader "Tetris/CartoonBoardIgnitionFlame"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Ignition Animation)]
        _RevealHeight ("Reveal Height (Bottom to Top)", Range(0, 1)) = 1.0
        _Intensity ("Flame Intensity", Range(0, 1)) = 1.0

        [Header(Cartoon Flame Palette)]
        _FlameCoreColor ("Flame Core Color", Color) = (1.0, 0.965, 0.702, 1.0)
        _FlameMidColor ("Flame Mid Color", Color) = (1.0, 0.690, 0.125, 1.0)
        _FlameEdgeColor ("Flame Edge Color", Color) = (1.0, 0.271, 0.094, 1.0)

        [Header(Frame Shape and Motion)]
        _BorderThickness ("Border Thickness", Range(0.02, 0.35)) = 0.14
        _BottomBoost ("Bottom Flame Boost", Range(0.0, 0.25)) = 0.08
        _TopBoost ("Top Flame Boost", Range(0.0, 0.25)) = 0.06
        _NoiseScale ("Flame Wave Scale", Range(2.0, 20.0)) = 8.0
        _ScrollSpeed ("Upward Scroll Speed", Range(0.5, 8.0)) = 3.2
        _Turbulence ("Wave Turbulence", Range(0.02, 0.25)) = 0.09
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+10"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            float _RevealHeight;
            float _Intensity;
            fixed4 _FlameCoreColor;
            fixed4 _FlameMidColor;
            fixed4 _FlameEdgeColor;

            float _BorderThickness;
            float _BottomBoost;
            float _TopBoost;
            float _NoiseScale;
            float _ScrollSpeed;
            float _Turbulence;

            // Hash & Value Noise nhẹ chạy nhanh trên cả PC và Mobile (Built-in 2D)
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash21(i);
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbmNoise(float2 p)
            {
                float v = 0.0;
                v += 0.65 * valueNoise(p);
                v += 0.35 * valueNoise(p * 2.15 + float2(3.7, 1.9));
                return v;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                if (_Intensity <= 0.001 || _RevealHeight <= 0.001)
                {
                    return fixed4(0, 0, 0, 0);
                }

                float2 uv = IN.texcoord;

                // Quét lửa từ chân bảng (y = 0) lên đỉnh bảng (y = 1) có mép lượn sóng hoạt hình
                float2 scrollUV = float2(uv.x * _NoiseScale, uv.y * _NoiseScale - _Time.y * _ScrollSpeed);
                float noise = fbmNoise(scrollUV);
                float detailNoise = valueNoise(scrollUV * 1.9 + float2(0.0, -_Time.y * (_ScrollSpeed * 0.7)));

                float sweepEdge = _RevealHeight + (noise - 0.5) * 0.14;
                float revealMask = 1.0 - smoothstep(sweepEdge - 0.06, sweepEdge + 0.02, uv.y);

                // Khoảng cách từ 4 cạnh khung bảng (giữ phần ruột bàn cờ trong suốt tuyệt đối)
                float distLeft   = uv.x;
                float distRight  = 1.0 - uv.x;
                float distBottom = uv.y;
                float distTop    = 1.0 - uv.y;

                // Tăng độ dày ngọn lửa ở chân bảng và đỉnh bảng giống concept Cartoon Fantasy Arcade
                float sideThickness   = _BorderThickness + (noise - 0.35) * _Turbulence;
                float bottomThickness = _BorderThickness + _BottomBoost + (noise - 0.3) * _Turbulence;
                float topThickness    = _BorderThickness + _TopBoost + (detailNoise - 0.3) * (_Turbulence * 1.25);

                float edgeLeft   = 1.0 - smoothstep(sideThickness * 0.35, sideThickness, distLeft);
                float edgeRight  = 1.0 - smoothstep(sideThickness * 0.35, sideThickness, distRight);
                float edgeBottom = 1.0 - smoothstep(bottomThickness * 0.35, bottomThickness, distBottom);
                float edgeTop    = 1.0 - smoothstep(topThickness * 0.35, topThickness, distTop);

                float frameHeat = max(max(edgeLeft, edgeRight), max(edgeBottom, edgeTop));

                // Tạo các dải màu hoạt hình sắc cạnh (Cel-Shaded Cartoon Bands)
                float shapedHeat = frameHeat * (0.72 + 0.45 * noise) * revealMask;

                float outerBand = smoothstep(0.18, 0.28, shapedHeat);
                float midBand   = smoothstep(0.46, 0.56, shapedHeat);
                float coreBand  = smoothstep(0.74, 0.84, shapedHeat);

                fixed3 col = lerp(_FlameEdgeColor.rgb, _FlameMidColor.rgb, midBand);
                col = lerp(col, _FlameCoreColor.rgb, coreBand);

                // Lóe sáng nhẹ tại đầu mũi quét khi lửa đang bùng từ chân lên đỉnh
                float sweepTip = (1.0 - abs(uv.y - _RevealHeight) * 8.0) * step(_RevealHeight, 0.99);
                col += _FlameCoreColor.rgb * saturate(sweepTip) * 0.35 * outerBand;

                float alpha = saturate(outerBand * _Intensity * IN.color.a);
                return fixed4(col, alpha);
            }
            ENDCG
        }
    }
}
