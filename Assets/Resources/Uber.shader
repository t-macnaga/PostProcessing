Shader "PostEffect/Uber"
{
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #pragma shader_feature GRAYSCALE
        #pragma shader_feature BLOOM
        #pragma shader_feature DOF
        #pragma shader_feature GAUSSIAN_BLUR
            
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _MainTex_TexelSize;
        sampler2D _SourceTex;
        sampler2D _HalfTex;
        float4 _HalfTex_TexelSize;
        sampler2D _QuaterTex;
        sampler2D _SixteenthTex;

        // _Thresholdを削除して_FilterParamsを追加
        half4 _FilterParams;
        float _Intensity;
        uniform half      _Radius;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        half3 sampleMain(float2 uv){
            return tex2D(_MainTex, uv).rgb;
        }

        half3 sampleBox (float2 uv, float delta) {
            float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
            half3 sum = sampleMain(uv + offset.xy) + sampleMain(uv + offset.zy) + sampleMain(uv + offset.xw) + sampleMain(uv + offset.zw);
            return sum * 0.25;
        }

        half getBrightness(half3 color){
            return Luminance(color);
            // return max(color.r, max(color.g, color.b));
        }

        // half4 Blur( half2 dir,v2f i)
        // {
        //     half4 color = tex2D(_MainTex, i.uv);
        //     float weights[5] = { 0.22702702702, 0.19459459459, 0.12162162162, 0.05405405405, 0.01621621621 };
        //     float2 offset = dir * _MainTex_TexelSize.xy * 3;//_Radius;
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

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        ENDCG


        Cull Off
        ZTest Always
        ZWrite Off

        Tags { "RenderType"="Opaque" }

        // 0: 適用するピクセル抽出用のパス
        Pass
        {
            CGPROGRAM

            fixed4 frag (v2f i) : SV_Target
            {
                // 色抽出にソフトニーを適用
                half4 col = 1;
                // col.rgb = sampleBox(i.uv, 1.0);
                col = tex2D(_MainTex, i.uv);
                half brightness = getBrightness(col.rgb);

                half soft = brightness - _FilterParams.y;
                soft = clamp(soft, 0, _FilterParams.z);
                soft = soft * soft * _FilterParams.w;
                half contribution = max(soft, brightness - _FilterParams.x);
                contribution /= max(brightness, 0.00001);
                return col * contribution;
            }

            ENDCG
        }

        // 1: ダウンサンプリング用のパス
        Pass
        {
            CGPROGRAM

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = 1;
                // col.rgb = Blur(half2(1,0),i);
                // col.rgb += Blur(half2(0,1),i);
                // col.rgb /= 2;
                // col.r
                col.rgb = sampleBox(i.uv, 1.0);
                return col;
            }

            ENDCG
        }

        // 2: アップサンプリング用のパス
        Pass
        {
            Blend One One

            CGPROGRAM

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = 1;
                col.rgb = sampleBox(i.uv, 1);
                return col;
            }

            ENDCG
        }

        // 3: 最後の一回のアップサンプリング用のパス
        Pass
        {
            CGPROGRAM

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_SourceTex, i.uv);
                col.rgb += sampleBox(i.uv, 1) * _Intensity;
                return col;
            }

            ENDCG
        }

        // 4: デバッグ用
        Pass
        {
            CGPROGRAM

            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = 1;
                col.rgb = sampleBox(i.uv, 0.5) * _Intensity;
                return col;
            }

            ENDCG
        }
      

            Cull Off ZWrite Off ZTest Always
        // 5: Gaussian Blur
        Pass {
            CGPROGRAM
            // #pragma vertex vert
            // #pragma fragment frag

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
                color.rgb = sampleBox(i.uv , 1);

                // // Gaussian Blur
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

            // 6: DOF
        Pass {
            CGPROGRAM
            // #pragma vertex vert
            // #pragma fragment frag

            // Properties
            // uniform sampler2D _MainTex;
            uniform sampler2D _BlurTex;
            uniform half      _Depth;
            uniform sampler2D _CameraDepthTexture;

            //------------------------------------------------------------------------
            // Fragment Shader
            //------------------------------------------------------------------------
            fixed4 frag(v2f i) : SV_Target {
                fixed4 color = tex2D(_MainTex, i.uv);

                // DOF
                float depth = tex2D(_CameraDepthTexture, i.uv).r;
                depth = 1.0 / (_ZBufferParams.x * depth + _ZBufferParams.y) * _Depth;
                float blur = saturate(depth * _ProjectionParams.z);
                color.rgb = lerp(color.rgb, tex2D(_BlurTex, i.uv).rgb, blur);

                return color;
            }
            ENDCG
        }
        
        // 7 : grayscale
        Pass {
            CGPROGRAM

            fixed4 frag(v2f i) : SV_Target {
                fixed4 color = tex2D(_MainTex, i.uv);
                return Luminance(color);
            }
            ENDCG
        }

        // 8 :Uber.
        Pass {
            CGPROGRAM

            half GetBright(float2 uv)
            {
                // half4 col=1;
                return tex2D(_QuaterTex, uv).r;
                // // 色抽出にソフトニーを適用
                // half4 col = 1;
                // // col.rgb = sampleBox(uv, 1.0);
                // // col = tex2D(_HalfTex, uv);
                // col = tex2D(_QuaterTex, uv);
                // // col = tex2D(_MainTex, uv);
                // // return col;
                // half brightness = getBrightness(col.rgb);

                // half soft = brightness - _FilterParams.y;
                // soft = clamp(soft, 0, _FilterParams.z);
                // soft = soft * soft * _FilterParams.w;
                // half contribution = max(soft, brightness - _FilterParams.x);
                // contribution /= max(brightness, 0.00001);
                // return contribution;
                // half4 result = col * contribution;
                // sampleBox()
            }
            
            // half3 SampleBox(float2 uv , float delta){
            //     // float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
            //     float4 offset = _HalfTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
            //     half3 sum = 0;
            //     sum += (uv + offset.xy).rgb;
            //     sum += GetBright(uv + offset.zy).rgb;
            //     sum += GetBright(uv + offset.xw).rgb;
            //     sum += GetBright(uv + offset.zw).rgb;
            //     return sum * 0.25;
            // }
        
            // half4 Blur( half2 dir,v2f i)
            // {
            //     half4 color = tex2D(_MainTex, i.uv);
            //     float weights[5] = { 0.22702702702, 0.19459459459, 0.12162162162, 0.05405405405, 0.01621621621 };
            //     float2 offset = dir * _MainTex_TexelSize.xy * 3;//_Radius;
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
            
            half3 SampleBoxFromBrightness(float2 uv , float delta){
                float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
                // float4 offset = _HalfTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
                half3 sum = 0;
                // sum += GetBright(uv + offset.xy).rgb;
                // sum += GetBright(uv + offset.zy).rgb;
                // sum += GetBright(uv + offset.xw).rgb;
                // sum += GetBright(uv + offset.zw).rgb;
                // half4 col = 1;
                // col.rgb = sampleBox(uv, delta);
                // half brightness = getBrightness(col.rgb);
                // half brightness = GetBright(uv);
                sum += tex2D(_MainTex, uv + offset.xy).rgb * GetBright(uv+offset.xy);
                sum += tex2D(_MainTex, uv + offset.zy).rgb * GetBright(uv+offset.zy);// brightness;
                sum += tex2D(_MainTex, uv + offset.xw).rgb * GetBright(uv+offset.xw);// brightness;
                sum += tex2D(_MainTex, uv + offset.zw).rgb * GetBright(uv+offset.zw);// brightness;
                return sum * 0.25;
            }

            // half3 SampleBoxFromBrightness(v2f i , float delta){
            //     // float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
            //     // float4 offset = _HalfTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
            //     half3 sum = 0;
            //     // sum += GetBright(uv + offset.xy).rgb;
            //     // sum += GetBright(uv + offset.zy).rgb;
            //     // sum += GetBright(uv + offset.xw).rgb;
            //     // sum += GetBright(uv + offset.zw).rgb;
            //     // half4 col = 1;
            //     // col.rgb = sampleBox(uv, delta);
            //     // half brightness = getBrightness(col.rgb);
            //     half brightness = GetBright(i.uv);
            //     // sum += tex2D(_MainTex, uv + offset.xy).rgb * GetBright(uv + offset.xy);
            //     // sum += tex2D(_MainTex, uv + offset.zy).rgb * GetBright(uv+offset.zy);// brightness;
            //     // sum += tex2D(_MainTex, uv + offset.xw).rgb * GetBright(uv+offset.xw);// brightness;
            //     // sum += tex2D(_MainTex, uv + offset.zw).rgb * GetBright(uv+offset.zw);// brightness;
            //     // return sum * 0.25;
            //     sum += Blur(half2(1,0),i);
            //     sum += Blur(half2(0,1),i);
            //     return sum * 0.5*brightness;
            // }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 finalColor = tex2D(_MainTex, i.uv);
                #ifdef GRAYSCALE
                finalColor = Luminance(finalColor);
                #endif

                #ifdef BLOOM

                // ここでやろうとしたが、失敗した。

                // // Down Sample
                // // half4 col = 1;
                // col.rgb = SampleBoxFromBrightness(i.uv , 0.5 );
                // col.rgb += SampleBoxFromBrightness(i.uv , 1.0 );
                // col.rgb += SampleBoxFromBrightness(i.uv , 1.5 );
                // col.rgb += SampleBoxFromBrightness(i.uv , 2.0 );
                // col.rgb *= 0.25;
                
                // Up Sample


                // // 最後の１回
                // half4 col = tex2D(_SourceTex, i.uv);
                // col.rgb += sampleBox(i.uv, 1) * _Intensity;
                // finalColor += col * _Intensity;
                // half4 col =1;
                // // col.rgb = SampleBoxFromBrightness(i,1.0);
                // col.rgb = SampleBoxFromBrightness(i.uv,1.0);
                // col.rgb += SampleBoxFromBrightness(i.uv,2.0);
                // col.rgb += SampleBoxFromBrightness(i.uv,4.0);
                // col.rgb += SampleBoxFromBrightness(i.uv,8.0);
                // // col.rgb += SampleBoxFromBrightness(i.uv,10.0);
                // // col.rgb *= 0.25;
                // finalColor += col * _Intensity;
                #endif

                #ifdef DOF
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}