#ifndef __BLOOM__
#define __BLOOM__

#include "Common.cginc"

float _Threshold;
float _Intensity;
sampler2D _BloomTex1;
sampler2D _BloomTex2;
sampler2D _BloomTex3;
sampler2D _BloomTex4;
sampler2D _BloomTex5;
sampler2D _BloomTex6;

fixed4 FragBloomCombine2(v2f i) : SV_Target {
    half4 bloom = tex2D(_BloomTex1, i.uv);
    bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
    bloom.rgb /=2;
    bloom.rgb *= _Intensity;
    return bloom;
}

fixed4 FragBloomCombine3(v2f i) : SV_Target {
    half4 bloom = tex2D(_BloomTex1, i.uv);
    bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex3, i.uv).rgb;
    bloom.rgb /=3;
    bloom.rgb *= _Intensity;
    return bloom;
}

fixed4 FragBloomCombine4(v2f i) : SV_Target {
    half4 bloom = tex2D(_BloomTex1, i.uv);
    bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex3, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex4, i.uv).rgb;
    bloom.rgb /=4;
    bloom.rgb *= _Intensity;
    return bloom;
}

fixed4 FragBloomCombine5(v2f i) : SV_Target {
    half4 bloom = tex2D(_BloomTex1, i.uv);
    bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex3, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex4, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex5, i.uv).rgb;
    bloom.rgb /=5;
    bloom.rgb *= _Intensity;
    return bloom;
}

fixed4 FragBloomCombine6(v2f i) : SV_Target {
    half4 bloom = tex2D(_BloomTex1, i.uv);
    bloom.rgb += tex2D(_BloomTex2, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex3, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex4, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex5, i.uv).rgb;
    bloom.rgb += tex2D(_BloomTex6, i.uv).rgb;
    bloom.rgb /=6;
    bloom.rgb *= _Intensity;
    return bloom;
}

#endif // __BLOOM__
