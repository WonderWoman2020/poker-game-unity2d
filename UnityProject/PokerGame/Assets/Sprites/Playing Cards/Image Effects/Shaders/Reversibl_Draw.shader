Shader "Custom/Cutout Double-sided"
{
Properties
{
_Cutoff ("Cutoff", Range(0,1)) = 0.5
_Color ("Color", Color) = (1,1,1,1)
_MainTex ("Albedo (RGB)", 2D) = "white" {}
_MainTex2("Texture2", 2D) = "white"{}
_Glossiness ("Smoothness", Range(0,1)) = 0.5
_Metallic ("Metallic", Range(0,1)) = 0.0
}
SubShader
{
Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
LOD 200

Cull Off

CGPROGRAM

#pragma surface surf Standard alphatest:_Cutoff addshadow fullforwardshadows
#pragma target 3.0

sampler2D _MainTex;
sampler2D _MainTex2;

struct Input
{
float2 uv_MainTex;
};

half _Glossiness;
half _Metallic;
fixed4 _Color;

void surf (Input IN, inout SurfaceOutputStandard o)
{
fixed4 c = tex2D (_MainTex2, IN.uv_MainTex) * _Color;
o.Albedo = c.rgb;
o.Metallic = _Metallic;
o.Smoothness = _Glossiness;
o.Alpha = c.a;
}

ENDCG

Cull Front

CGPROGRAM

#pragma surface surf Standard alphatest:_Cutoff fullforwardshadows vertex:vert
#pragma target 3.0

sampler2D _MainTex;


struct Input
{
float2 uv_MainTex;
};

void vert (inout appdata_full v)
{
v.normal.xyz = v.normal * -1;
}

half _Glossiness;
half _Metallic;
fixed4 _Color;


void surf (Input IN, inout SurfaceOutputStandard o)
{
fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
o.Albedo = c.rgb;
o.Metallic = _Metallic;
o.Smoothness = _Glossiness;
o.Alpha = c.a;
}

ENDCG
}
FallBack "Diffuse"
}