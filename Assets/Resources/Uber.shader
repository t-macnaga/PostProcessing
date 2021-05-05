Shader "Hidden/PostEffect/Uber"
{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
        #pragma multi_compile __ BLOOM
        #pragma multi_compile __ DOF FAKE_DOF
        // #pragma multi_compile __ CHROMATIC_ABERRATION
        // #pragma multi_compile __ VIGNETTE
        // #pragma multi_compile __ GRAYSCALE
            
        #include "Common.cginc"

        half _ChromaticAberrationSize;

        //DOF
        uniform sampler2D _BlurTex;
        uniform half      _Depth;
        uniform sampler2D _CameraDepthTexture;

        // Vignette
        half _VignetteIntensity;
        half _VignetteSmoothness;
        
        //Final Blur
        sampler2D _FinalBlurTex;

        ENDCG

        Tags { "RenderType"="Opaque" }

        Cull Off ZWrite Off ZTest Always

        // 0: DOF
        Pass {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment FragBlur

            fixed4 FragBlur(v2f i) : SV_Target {
                half4 color = 1;
                color.rgb = SampleBox(i.uv , _Radius);
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
                return color;
            }
            ENDCG
        }

        // 1: Final
        Pass {

            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment FragUber

            fixed4 FragUber(v2f i) : SV_TARGET{
                fixed4 color = tex2D(_MainTex,i.uv);

                half4 bloom = half4(0,0,0,0);
                float depth = 0;
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
                    depth = Luminance(color.rgb);
                }
                #endif

                #if DOF
                {
                    depth = tex2D(_CameraDepthTexture, i.uv).r;
                    depth = 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y) * _Depth;
                    float blur = saturate(depth * _ProjectionParams.z);
                    color.rgb = lerp(color.rgb, tex2D(_BlurTex, i.uv).rgb, blur);
                }
                #elif FAKE_DOF
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