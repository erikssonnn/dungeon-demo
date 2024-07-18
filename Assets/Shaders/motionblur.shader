Shader "Hidden/motionblur" {
    Properties {
        _MainTex ("Input", RECT) = "white" {}
        _BlurStrength ("", Float) = 0.5
        _BlurWidth ("", Float) = 0.5
    }

    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
       
            CGPROGRAM
   
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
 
            uniform sampler2D _MainTex;
            uniform half4 _MainTex_TexelSize;
            uniform half _BlurStrength;
            uniform half _BlurWidth;
            uniform half _imgWidth;
            uniform half _imgHeight;
 
            half4 frag (v2f_img i) : COLOR {
                half4 color = tex2D(_MainTex, i.uv);
       
                half samples[10];
                samples[0] = -0.08;
                samples[1] = -0.05;
                samples[2] = -0.03;
                samples[3] = -0.02;
                samples[4] = -0.01;
                samples[5] =  0.01;
                samples[6] =  0.02;
                samples[7] =  0.03;
                samples[8] =  0.05;
                samples[9] =  0.08;
       
                half2 dir = 0.5 * half2(_imgHeight,_imgWidth) - i.uv;
                half dist = sqrt(dir.x*dir.x + dir.y*dir.y);
                dir = dir/dist;
       
                half4 sum = color;
                for(int n = 0; n < 10; n++) {
                    sum += tex2D(_MainTex, i.uv + dir * samples[n] * _BlurWidth * _imgWidth);
                }
       
                sum *= 1.0/11.0;
                half t = saturate(dist * _BlurStrength);
                return lerp(color, sum, t);
            }
            ENDCG
        }
    }
}