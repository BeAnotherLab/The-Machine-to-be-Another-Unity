Shader "Hidden/AVProLiveCamera/CompositeUYVY_2_RGBA" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TextureWidth ("Texure Width", Float) = 256.0
		_TextureScaleOffset ("Texure Scale Offset", Vector) = (1.0, 1.0, 0.0, 0.0)
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
#pragma multi_compile HORIZONTAL_FLIP_ON HORIZONTAL_FLIP_OFF
#pragma multi_compile AVPRO_GAMMACORRECTION AVPRO_GAMMACORRECTION_OFF
#pragma multi_compile YCBCR_RANGE_LIMITED YCBCR_RANGE_FULL
#include "UnityCG.cginc"
#include "AVProLiveCamera_Shared.cginc"

uniform sampler2D _MainTex;
uniform float _TextureWidth;
uniform float4 _TextureScaleOffset;
uniform float4 _MainTex_TexelSize;

struct v2f {
	float4 pos : POSITION;
	float3 uv : TEXCOORD0;
};

v2f vert( appdata_img v )
{
	v2f o;
	o.pos = UnityObjectToClipPos (v.vertex);
	o.uv.xy = (v.texcoord.xy * _TextureScaleOffset.xy + _TextureScaleOffset.zw);
	
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
	
	o.uv.z = v.vertex.x * _TextureWidth * 0.5;

	return o;
}

float4 frag (v2f i) : COLOR
{
	float4 col = tex2D(_MainTex, i.uv.xy);
#if defined(SWAP_RED_BLUE_ON)
	col = col.bgra;
#endif

#if defined(HORIZONTAL_FLIP_ON)
	col = col.bgra;
#endif

	//uyvy
	float y = col.w;
	float u = col.x;
	float v = col.z;
	
	if (frac(i.uv.z) < 0.5 )
	{
		// ODD PIXELS
		y = col.y;
		
		/*float4 col2 = tex2D(_MainTex, i.uv.xy + float2(_MainTex_TexelSize.x, 0.0));
#if defined(SWAP_RED_BLUE_ON)
		col2 = col2.bgra;
#endif
		u = (col.x + col2.x) * 0.5;
		v = (col.z + col2.z) * 0.5;*/
	}

#if defined(YCBCR_RANGE_LIMITED)
	float4 oCol = convertYUV_BT709_Limited_RGB(y, u, v);
#else
	float4 oCol = convertYUV_BT709_Full_RGB(y, u, v);
#endif

#if defined(AVPRO_GAMMACORRECTION)
	oCol.rgb = TransferSRGB_GammaToLinear(oCol.rgb);
#endif

	return float4(oCol.rgb, 1);
} 
ENDCG
		}
	}
	
	FallBack Off
}