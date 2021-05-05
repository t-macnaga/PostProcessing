Shader "Hidden/PostEffect/Bloom"
{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
        // #pragma multi_compile __ BLOOM
        // #pragma multi_compile __ DOF FAKE_DOF_WITH_BLOOM
        // #pragma multi_compile __ CHROMATIC_ABERRATION
        // #pragma multi_compile __ VIGNETTE
        // #pragma shader_feature GAUSSIAN_BLUR

        #include "Common.cginc"
        #include "Bloom.cginc"
        #include "UnityCG.cginc"

        // sampler2D _MainTex;
        // float4 _MainTex_ST;
        // float4 _MainTex_TexelSize;
        // Chromatic Aberration
        // half _ChromaticAberrationSize;

        // Bloom
        // sampler2D _BloomTex1;
        // sampler2D _BloomTex2;
        // sampler2D _BloomTex3;
        // sampler2D _BloomTex4;
        // sampler2D _BloomTex5;
        // sampler2D _BloomTex6;

        //DOF
        // uniform sampler2D _BlurTex;
        // uniform half      _Depth;
        // uniform sampler2D _CameraDepthTexture;

        // Vignette
        // half _VignetteIntensity;
        // half _VignetteSmoothness;
        
        //Final Blur
        // sampler2D _FinalBlurTex;

        // half4 _FilterParams;
        // float _Threshold;
        // float _Intensity;
        // uniform half      _Radius;

        // struct appdata
        // {
        //     float4 vertex : POSITION;
        //     float2 uv : TEXCOORD0;
        // };

        // struct v2f
        // {
        //     float2 uv : TEXCOORD0;
        //     float4 vertex : SV_POSITION;
        // };

        // half3 sampleMain(float2 uv){
        //     return tex2D(_MainTex, uv).rgb;
        // }

        // half3 sampleBox (float2 uv, float delta) {
        //     float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
        //     half3 sum = sampleMain(uv + offset.xy) + sampleMain(uv + offset.zy) + sampleMain(uv + offset.xw) + sampleMain(uv + offset.zw);
        //     return sum * 0.25;
        // }

        // half getBrightness(half3 color){
        //     return Luminance(color);
        // }

        // half4 Blur( half2 dir,v2f i)
        // {
        //     half4 color = tex2D(_MainTex, i.uv);
        //     float weights[5] = { 0.22702702702, 0.19459459459, 0.12162162162, 0.05405405405, 0.01621621621 };
        //     float2 offset = dir * _MainTex_TexelSize.xy * _Radius;
        //     color.rgb *= weights[0];
        //     color.rgb += tex2D(_MainTex, i.uv + offset      ).rgb * weights[1];
        //     color.rgb += tex2D(_MainTex, i.uv - offset      ).rgb * weights[1];
        //     color.rgb += tex2D(_MainTex, i.uv + offset * 2.0).rgb * weights[2];
        //     color.rgb += tex2D(_MainTex, i.uv - offset * 2.0).rgb * weights[2];
        //     color.rgb += tex2D(_MainTex, i.uv + offset * 3.0).rgb * weights[3];
        //     color.rgb += tex2D(_MainTex, i.uv - offset * 3.0).rgb * weights[3];
        //     color.rgb += tex2D(_MainTex, i.uv + offset * 4.0).rgb * weights[4];
        //     color.rgb += tex2D(_MainTex, i.uv - offset * 4.0).rgb * weights[4];
        //     return color;
        // }

        // v2f vert (appdata v)
        // {
        //     v2f o;
        //     o.vertex = UnityObjectToClipPos(v.vertex);
        //     o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        //     return o;
        // }

        ENDCG

        Cull Off
        ZTest Always
        ZWrite Off

        Tags { "RenderType"="Opaque" }

        // 0: 適用するピクセル抽出用のパス
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = 1;
                col = tex2D(_MainTex, i.uv);
                // col.rgb = sampleBox(i.uv, 1.0);
                
                #if UNITY_COLORSPACE_GAMMA
                col.rgb = GammaToLinearSpace(col.rgb);
                #endif

                // half brightness = getBrightness(col.rgb);
                // half soft = brightness - _FilterParams.y;
                // soft = clamp(soft, 0, _FilterParams.z);
                // soft = soft * soft * _FilterParams.w;
                // half contribution = max(soft, brightness - _FilterParams.x);
                // contribution /= max(brightness, 0.00001);
                
                col.a = getBrightness(col.rgb);
                // 白飛びを防ぐ
                col.rgb = max(col.rgb - _Threshold, 0);
                // col.a += max(col.a - _Threshold, 0);
                return col;
            }

            ENDCG
        }

        // 1: ダウンサンプリング用のパス
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = 1;
                // col.rgb = Blur(half2(1,0),i);
                // col.rgb += Blur(half2(0,1),i);
                // col.rgb /= 2;
                col.rgb = sampleBox(i.uv, 1.0);
                return col;
            }

            ENDCG
        }

        // 2 : BloomCombine2
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex vert
            #pragma fragment FragBloomCombine2
            ENDCG
        }

        // 3 : BloomCombine3
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex vert
            #pragma fragment FragBloomCombine3
            ENDCG
        }

        // 4 : BloomCombine4
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex vert
            #pragma fragment FragBloomCombine4
            ENDCG
        }

        // 5 : BloomCombine5
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex vert
            #pragma fragment FragBloomCombine5
            ENDCG
        }

        // 6 : BloomCombine6
        Pass {
            CGPROGRAM
            #include "Bloom.cginc"
            #pragma vertex vert
            #pragma fragment FragBloomCombine6
            ENDCG
        }
    }
}