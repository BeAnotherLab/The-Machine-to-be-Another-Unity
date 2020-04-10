// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/AVProLiveCamera/IMGUI"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}

	SubShader
	{
		Tags { "ForceSupported" = "True" "RenderType" = "Overlay" }

		Lighting Off
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ APPLY_GAMMA

			#include "UnityCG.cginc"
			#include "AVProLiveCamera_Shared.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float2 _Flip;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				if (_Flip.x < 0.0)
				{
					o.texcoord.x = (1.0 - o.texcoord.x);
				}
				if (_Flip.y < 0.0)
				{
					o.texcoord.y = (1.0 - o.texcoord.y);
				}
				o.color = v.color;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord.xy);
#if APPLY_GAMMA
				col.rgb = linearToGammaFast(col.rgb);
#endif
				return col * i.color;
			}
			ENDCG
		}
	}

	Fallback off
}