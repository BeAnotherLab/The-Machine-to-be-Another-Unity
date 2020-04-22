Shader "Hidden/AVProLiveCamera/Deinterlace" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {

	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }
		
				
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma exclude_renderers flash xbox360 ps3 gles
//#pragma fragmentoption ARB_precision_hint_fastest
#pragma fragmentoption ARB_precision_hint_nicest
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform half4 _MainTex_TexelSize;

fixed4 frag (v2f_img i) : COLOR
{
	float4 c0 = tex2D(_MainTex, i.uv);

	float2 h = float2(0 , _MainTex_TexelSize.y);
	
	float4 c1 = tex2D(_MainTex, i.uv - h);
	float4 c2 = tex2D(_MainTex, i.uv + h);

	return (c0*2+c1+c2)/4;
}
ENDCG

	}
}

Fallback off

}