// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AVProVideo/IMGUI/Texture Transparent"
{
	Properties
	{
		_MainTex("Texture", any) = "" {}
		_VertScale("Vertical Scale", Range(-1, 1)) = 1.0
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}

	SubShader
	{
		Tags { "ForceSupported" = "True" "RenderType" = "Overlay" }

		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA

			#include "UnityCG.cginc"
			#include "AVProVideo.cginc"

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
				float4 texcoord : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform float _VertScale;

			v2f vert(appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = OffsetAlphaPackingUV(_MainTex_TexelSize.xy, TRANSFORM_TEX(v.texcoord, _MainTex), _VertScale < 0.0);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// Sample RGB
				fixed4 col = tex2D(_MainTex, i.texcoord.xy);
#if APPLY_GAMMA
				col.rgb = pow(col.rgb, 1.0 / 2.2);
#endif


#if ALPHAPACK_TOP_BOTTOM | ALPHAPACK_LEFT_RIGHT
				// Sample the alpha
				fixed4 alpha = tex2D(_MainTex, i.texcoord.zw);
				
#if APPLY_GAMMA
				alpha.rgb = pow(alpha.rgb, 1.0 / 2.2);
#endif
				col.a = (alpha.r + alpha.g + alpha.b) / 3.0;
#endif

				return col * i.color;
			}
			ENDCG
		}
	}

	Fallback off
}