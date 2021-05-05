#ifndef __COMMON__
#define __COMMON__

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
float4 _MainTex_TexelSize;
uniform half _Radius;

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

half3 SampleMain(float2 uv){
    return tex2D(_MainTex, uv).rgb;
}

half3 SampleBox (float2 uv, float delta) {
    float4 offset = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
    half3 sum = SampleMain(uv + offset.xy) + SampleMain(uv + offset.zy) + SampleMain(uv + offset.xw) + SampleMain(uv + offset.zw);
    return sum * 0.25;
}

half4 Blur( half2 dir,v2f i)
{
    half4 color = tex2D(_MainTex, i.uv);
    float weights[5] = { 0.22702702702, 0.19459459459, 0.12162162162, 0.05405405405, 0.01621621621 };
    float2 offset = dir * _MainTex_TexelSize.xy * _Radius;
    color.rgb *= weights[0];
    color.rgb += tex2D(_MainTex, i.uv + offset      ).rgb * weights[1];
    color.rgb += tex2D(_MainTex, i.uv - offset      ).rgb * weights[1];
    color.rgb += tex2D(_MainTex, i.uv + offset * 2.0).rgb * weights[2];
    color.rgb += tex2D(_MainTex, i.uv - offset * 2.0).rgb * weights[2];
    color.rgb += tex2D(_MainTex, i.uv + offset * 3.0).rgb * weights[3];
    color.rgb += tex2D(_MainTex, i.uv - offset * 3.0).rgb * weights[3];
    color.rgb += tex2D(_MainTex, i.uv + offset * 4.0).rgb * weights[4];
    color.rgb += tex2D(_MainTex, i.uv - offset * 4.0).rgb * weights[4];
    return color;
}

v2f Vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}


#endif // __COMMON__
