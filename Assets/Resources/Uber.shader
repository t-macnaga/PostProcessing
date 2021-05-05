Shader "Hidden/PostEffect/Uber"
{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
        // #pragma vertex vert
        // #pragma fragment frag
        // #pragma multi_compile __ GRAYSCALE
        #pragma multi_compile __ BLOOM
        #pragma multi_compile __ DOF FAKE_DOF_WITH_BLOOM
        #pragma multi_compile __ CHROMATIC_ABERRATION
        #pragma multi_compile __ VIGNETTE
        // #pragma shader_feature GAUSSIAN_BLUR
            
        #include "Common.cginc"
        // #include "UnityCG.cginc"

        // sampler2D _MainTex;
        // float4 _MainTex_ST;
        // float4 _MainTex_TexelSize;
        // Chromatic Aberration
        half _ChromaticAberrationSize;

        // Bloom
        // sampler2D _BloomTex1;
        // sampler2D _BloomTex2;
        // sampler2D _BloomTex3;
        // sampler2D _BloomTex4;
        // sampler2D _BloomTex5;
        // sampler2D _BloomTex6;

        //DOF
        uniform sampler2D _BlurTex;
        uniform half      _Depth;
        uniform sampler2D _CameraDepthTexture;

        // Vignette
        half _VignetteIntensity;
        half _VignetteSmoothness;
        
        //Final Blur
        sampler2D _FinalBlurTex;

        // _Thresholdを削除して_FilterParamsを追加
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
        // Pass
        // {
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     fixed4 frag (v2f i) : SV_Target
        //     {
        //         half4 col = 1;
        //         col = tex2D(_MainTex, i.uv);
                
        //         #if UNITY_COLORSPACE_GAMMA
        //         col.rgb = GammaToLinearSpace(col.rgb);
        //         #endif

        //         // half brightness = getBrightness(col.rgb);
        //         // half soft = brightness - _FilterParams.y;
        //         // soft = clamp(soft, 0, _FilterParams.z);
        //         // soft = soft * soft * _FilterParams.w;
        //         // half contribution = max(soft, brightness - _FilterParams.x);
        //         // contribution /= max(brightness, 0.00001);
                
        //         col.a = getBrightness(col.rgb);
        //         // 白飛びを防ぐ
        //         col.rgb = max(col.rgb - _Threshold, 0);
        //         // col.a += max(col.a - _Threshold, 0);
        //         return col;
        //     }

        //     ENDCG
        // }

        // 1: ダウンサンプリング用のパス
        // Pass
        // {
        //     CGPROGRAM
        //     #pragma vertex vert
        //     #pragma fragment frag

        //     fixed4 frag (v2f i) : SV_Target
        //     {
        //         half4 col = 1;
        //         // col.rgb = Blur(half2(1,0),i);
        //         // col.rgb += Blur(half2(0,1),i);
        //         // col.rgb /= 2;
        //         col.rgb = sampleBox(i.uv, 1.0);
        //         return col;
        //     }

        //     ENDCG
        // }

        Cull Off ZWrite Off ZTest Always
        // 2: Gaussian Blur
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Properties
            // uniform sampler2D _MainTex;
            // uniform float4    _MainTex_TexelSize;
            uniform half2     _Direction;
            // uniform half      _Radius;

            //------------------------------------------------------------------------
            // Fragment Shader
            //------------------------------------------------------------------------
            fixed4 frag(v2f i) : SV_Target {
                half4 color = 1;//tex2D(_MainTex, i.uv);
                color.rgb = sampleBox(i.uv , _Radius);
                #if BLOOM
                    #if UNITY_COLORSPACE_GAMMA
                    {
                        color.rgb = GammaToLinearSpace(color.rgb);
                        color.rgb += tex2D(_FinalBlurTex,i.uv);
                        color.rgb = LinearToGammaSpace(color.rgb);
                    }
                    #else
                    {
                        color.rgb += tex2D(_FinalBlurTex,i.uv);
                    }
                    #endif
                #endif
                // color.rgb = sampleBox(i.uv , 1);
                // color.rgb = sampleBox(i.uv , 2);
                // color.rgb += sampleBox(i.uv , 2);
                // color.rgb *= 0.5;

                // // Gaussian Blur
                // half4 color = tex2D(_MainTex, i.uv);
                // float weights[5] = { 0.22702702702, 0.19459459459, 0.12162162162, 0.05405405405, 0.01621621621 };
                // float2 offset = _Direction * _MainTex_TexelSize.xy * _Radius;
                // color.rgb *= weights[0];
                // color.rgb += tex2D(_MainTex, i.uv + offset      ).rgb * weights[1];
                // color.rgb += tex2D(_MainTex, i.uv - offset      ).rgb * weights[1];
                // color.rgb += tex2D(_MainTex, i.uv + offset * 2.0).rgb * weights[2];
                // color.rgb += tex2D(_MainTex, i.uv - offset * 2.0).rgb * weights[2];
                // color.rgb += tex2D(_MainTex, i.uv + offset * 3.0).rgb * weights[3];
                // color.rgb += tex2D(_MainTex, i.uv - offset * 3.0).rgb * weights[3];
                // color.rgb += tex2D(_MainTex, i.uv + offset * 4.0).rgb * weights[4];
                // color.rgb += tex2D(_MainTex, i.uv - offset * 4.0).rgb * weights[4];

                return color;
            }
            ENDCG
        }

        // 3 : BloomCombine
        // Pass {
        //     CGPROGRAM
        //     #include "Bloom.cginc"
        //     #pragma vertex vert
        //     #pragma fragment FragBloomCombine6
        //     // fixed4 frag(v2f i) : SV_Target {
        //     //     fixed4 finalColor = 0;
        //     //     #ifdef BLOOM
        //     //         half4 bloom = tex2D(_BloomTex1, i.uv);
        //     //         bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
        //     //         bloom.rgb += tex2D(_BloomTex3, i.uv).rgb;
        //     //         bloom.rgb += tex2D(_BloomTex4, i.uv).rgb;
        //     //         bloom.rgb += tex2D(_BloomTex5, i.uv).rgb;
        //     //         bloom.rgb += tex2D(_BloomTex6, i.uv).rgb;
        //     //         bloom.rgb /=6;
        //     //         bloom.rgb *= _Intensity;
        //     //         finalColor += bloom;
        //     //     #endif
        //     //     return finalColor;
        //     // }
        //     ENDCG
        // }

        // 3: Final
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            fixed4 frag(v2f i) : SV_TARGET{
                fixed4 color = tex2D(_MainTex,i.uv);

                half4 bloom = half4(0,0,0,0);
                float depth = 0;//bloom.a;
                #if BLOOM
                {
                    bloom = tex2D(_FinalBlurTex,i.uv);
                    depth = bloom.a;
                    // Combine bloomをここでおこなうとモアレが発生しにくい。
                    // // depth = getBrightness(color.rgb);// color.r;
                    // bloom = tex2D(_BloomTex1,i.uv);
                    // depth = bloom.a;
                    // bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
                    // bloom.rgb += tex2D(_BloomTex3, i.uv).rgb;
                    // bloom.rgb += tex2D(_BloomTex4, i.uv).rgb;
                    // bloom.rgb += tex2D(_BloomTex5, i.uv).rgb;
                    // bloom.rgb += tex2D(_BloomTex6, i.uv).rgb;
                    // bloom.rgb /= 6;
                    // bloom.rgb *= _Intensity;
                    #if UNITY_COLORSPACE_GAMMA
                    {
                        color.rgb = GammaToLinearSpace(color.rgb);
                        color.rgb += bloom.rgb;
                        color.rgb = LinearToGammaSpace(color.rgb);
                    }
                    #else
                    {
                        color.rgb += bloom.rgb;
                    }
                    #endif
                }
                #else
                {
                    depth = getBrightness(color.rgb);
                }
                #endif

                #if DOF
                {
                    depth = tex2D(_CameraDepthTexture, i.uv).r;
                    depth = 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y) * _Depth;
                    float blur = saturate(depth * _ProjectionParams.z);
                    color.rgb = lerp(color.rgb, tex2D(_BlurTex, i.uv).rgb, blur);
                }
                #elif FAKE_DOF_WITH_BLOOM
                {
                    color.rgb = depth > _Depth ? color.rgb : tex2D(_BlurTex, i.uv).rgb;
                }
                #endif
                
                #if CHROMATIC_ABERRATION // inspired by: https://light11.hatenadiary.com/entry/2018/06/20/000151
                {
                    half2 uvBase = i.uv - 0.5h;
                    // R値を拡大したものに置き換える
                    half2 uvR = uvBase * (1.0h - _ChromaticAberrationSize * 2.0h) + 0.5h;
                    color.r = tex2D(_MainTex, uvR).r;
                    // G値を拡大したものに置き換える
                    half2 uvG = uvBase * (1.0h - _ChromaticAberrationSize) + 0.5h;
                    color.g = tex2D(_MainTex, uvG).g;
                }
                #endif


                // inspired by : https://www.shadertoy.com/view/lsKSWR
                #if VIGNETTE
                {
                    half2 uv = i.uv;
                    uv *=  1.0 - uv.yx;   //vec2(1.0)- uv.yx; -> 1.-u.yx; Thanks FabriceNeyret !
                    float vig = uv.x*uv.y * _VignetteIntensity; // multiply with sth for intensity
                    vig = pow(vig, _VignetteSmoothness );// 0.25); // change pow for modifying the extend of the  vignette
                    color *= vig.xxxx;
                }
                #endif

                #if GRAYSCALE
                {
                    color = Luminance(color);
                }
                #endif
                return color;
            }
            ENDCG
        }
    }
}