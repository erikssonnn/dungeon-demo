Shader "Hidden/dither"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {

        CGINCLUDE
            #include "UnityCG.cginc"

            struct VertexData {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D _MainTex;
            SamplerState point_clamp_sampler;
            float4 _MainTex_TexelSize;

            v2f vp(VertexData v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
        ENDCG

        Pass {
            CGPROGRAM
            #pragma vertex vp
            #pragma fragment fp

            float _Spread;
            float _Threshold;
            int _RedColorCount, _GreenColorCount, _BlueColorCount , _BayerLevel;

            static const int bayer2[2 * 2] = {
                0, 2,
                3, 1
            };

            static const int bayer4[4 * 4] = {
                0, 8, 2, 10,
                12, 4, 14, 6,
                3, 11, 1, 9,
                15, 7, 13, 5
            };

            static const int bayer8[8 * 8] = {
                0, 32, 8, 40, 2, 34, 10, 42,
                48, 16, 56, 24, 50, 18, 58, 26,  
                12, 44,  4, 36, 14, 46,  6, 38, 
                60, 28, 52, 20, 62, 30, 54, 22,  
                3, 35, 11, 43,  1, 33,  9, 41,  
                51, 19, 59, 27, 49, 17, 57, 25, 
                15, 47,  7, 39, 13, 45,  5, 37, 
                63, 31, 55, 23, 61, 29, 53, 21
            };

            float GetBayer2(int x, int y) {
                return float(bayer2[(x % 2) + (y % 2) * 2]) * (1.0f / 4.0f) - 0.5f;
            }

            float GetBayer4(int x, int y) {
                return float(bayer4[(x % 4) + (y % 4) * 4]) * (1.0f / 16.0f) - 0.5f;
            }

            float GetBayer8(int x, int y) {
                return float(bayer8[(x % 8) + (y % 8) * 8]) * (1.0f / 64.0f) - 0.5f;
            }

            fixed4 fp(v2f i) : SV_Target {
                float4 col = _MainTex.Sample(point_clamp_sampler, i.uv);

                // Get neighboring pixels
                float4 leftCol = _MainTex.Sample(point_clamp_sampler, i.uv - float2(_MainTex_TexelSize.x, 0));
                float4 rightCol = _MainTex.Sample(point_clamp_sampler, i.uv + float2(_MainTex_TexelSize.x, 0));
                float4 topCol = _MainTex.Sample(point_clamp_sampler, i.uv - float2(0, _MainTex_TexelSize.y));
                float4 bottomCol = _MainTex.Sample(point_clamp_sampler, i.uv + float2(0, _MainTex_TexelSize.y));

                // Calculate color differences
                float4 colorDiff = abs(col - leftCol) + abs(col - rightCol) + abs(col - topCol) + abs(col - bottomCol);

                // Check if color difference exceeds threshold
                bool applyDither = any(colorDiff > _Threshold);

                // Apply dithering if necessary
                if (applyDither) {
                    int x = i.uv.x * _MainTex_TexelSize.z;
                    int y = i.uv.y * _MainTex_TexelSize.w;

                    float bayerValues[3] = { 0, 0, 0 };
                    bayerValues[0] = GetBayer2(x, y);
                    bayerValues[1] = GetBayer4(x, y);
                    bayerValues[2] = GetBayer8(x, y);

                    col += _Spread * bayerValues[_BayerLevel];

                    col.r = floor((_RedColorCount - 1.0f) * col.r + 0.5) / (_RedColorCount - 1.0f);
                    col.g = floor((_GreenColorCount - 1.0f) * col.g + 0.5) / (_GreenColorCount - 1.0f);
                    col.b = floor((_BlueColorCount - 1.0f) * col.b + 0.5) / (_BlueColorCount - 1.0f);
                }

                return col;
            }
            ENDCG
        }
    }
}
