Shader "Hidden/PostEffect/Bloom"
{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
        #include "Bloom.cginc"
        ENDCG

        Cull Off
        ZTest Always
        ZWrite Off

        Tags { "RenderType"="Opaque" }

        // 0: 
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment FragLum

            fixed4 FragLum (v2f i) : SV_Target
            {
                half4 col = 1;
                col = tex2D(_MainTex, i.uv);
                
                #if UNITY_COLORSPACE_GAMMA
                col.rgb = GammaToLinearSpace(col.rgb);
                #endif

                col.a = Luminance(col.rgb);
                // 白飛びを防ぐ
                col.rgb = max(col.rgb - _Threshold, 0);
                return col;
            }

            ENDCG
        }

        // 1: 
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDownSample

            fixed4 FragDownSample (v2f i) : SV_Target
            {
                half4 col = 1;
                // col.rgb = Blur(half2(1,0),i);
                // col.rgb += Blur(half2(0,1),i);
                // col.rgb /= 2;
                col.rgb = SampleBox(i.uv, 1.0);
                return col;
            }

            ENDCG
        }

        // 2 : BloomCombine2
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex Vert
            #pragma fragment FragBloomCombine2
            ENDCG
        }

        // 3 : BloomCombine3
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex Vert
            #pragma fragment FragBloomCombine3
            ENDCG
        }

        // 4 : BloomCombine4
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex Vert
            #pragma fragment FragBloomCombine4
            ENDCG
        }

        // 5 : BloomCombine5
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex Vert
            #pragma fragment FragBloomCombine5
            ENDCG
        }

        // 6 : BloomCombine6
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex Vert
            #pragma fragment FragBloomCombine6
            ENDCG
        }
    }
}