// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/AVProLiveCamera/CompositeYUV_YV12" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainU ("Base (RGB)", 2D) = "white" {}
		_MainV ("Base (RGB)", 2D) = "white" {}
		_TextureWidth ("Texure Width", Float) = 256.0
	}
	SubShader 
	{
		Pass
		{ 
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
		
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma exclude_renderers flash xbox360 ps3 gles
//#pragma fragmentoption ARB_precision_hint_fastest
#pragma fragmentoption ARB_precision_hint_nicest
#pragma multi_compile SWAP_RED_BLUE_ON SWAP_RED_BLUE_OFF
#include "UnityCG.cginc"
#include "AVProLiveCamera_Shared.cginc"

uniform sampler2D _MainTex;
uniform sampler2D _MainU;
uniform sampler2D _MainV;
float _TextureWidth;
float4 _MainTex_ST;
float4 _MainU_ST;
float4 _MainTex_TexelSize;

struct myappdata {
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};

struct v2f {
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uv2 : TEXCOORD1;
};

v2f vert( myappdata v )
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
	o.uv2.xy = TRANSFORM_TEX(v.texcoord1, _MainU);
	
	// On D3D when AA is used, the main texture & scene depth texture
	// will come out in different vertical orientations.
	// So flip sampling of the texture when that is the case (main texture
	// texel size will have negative Y).
	#if SHADER_API_D3D9
	if (_MainTex_TexelSize.y < 0)
	{
		o.uv.y = 1-o.uv.y;
	}
	#endif
	

	return o;
}

float4 frag (v2f i) : COLOR
{
	float4 lumaPacked = tex2D(_MainTex, i.uv);
	
	float f = (i.uv.x * _TextureWidth);
	float fr = frac(f);
	float y = lumaPacked.r;
	if (fr >= 0.25)
		y = lumaPacked.g;
	if (fr >= 0.5)
		y = lumaPacked.b;
	if (fr >= 0.75)
		y = lumaPacked.a;
		
	float4 uPacked = tex2D(_MainU, i.uv2);
	float4 vPacked = tex2D(_MainV, i.uv2);
	
	fr = frac(i.uv.x * _TextureWidth / 2);
	float u = uPacked.r;
	float v = vPacked.r;
	if (fr >= 0.25)
	{
		u = uPacked.g;
		v = vPacked.g;
	}
	if (fr >= 0.5)
	{
		u = uPacked.b;
		v = vPacked.b;
	}
	if (fr >= 0.75)
	{
		u = uPacked.a;	
		v = vPacked.a;
	}

	return convertYUV(y, v, u);
} 

ENDCG
		}
	}
	
	FallBack Off
}