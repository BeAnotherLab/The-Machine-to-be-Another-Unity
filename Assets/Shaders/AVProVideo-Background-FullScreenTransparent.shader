Shader "AVProVideo/Background/Full Screen Transparent"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_Color("Main Color", Color) = (1,1,1,1)
		[KeywordEnum(None, Top_Bottom, Left_Right)] AlphaPack("Alpha Pack", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}
	SubShader
	{
		Tags { "Queue"="Background+1" "RenderType" = "Transparent" }
		LOD 100
		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma multi_compile ALPHAPACK_NONE ALPHAPACK_TOP_BOTTOM ALPHAPACK_LEFT_RIGHT
			#pragma multi_compile __ APPLY_GAMMA

			#include "UnityCG.cginc"
			#include "AVProVideo.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			uniform fixed4 _Color;

			v2f vert (appdata_img v)
			{
				v2f o;

				float2 scale = ScaleZoomToFit(_ScreenParams.x, _ScreenParams.y, _MainTex_TexelSize.z, _MainTex_TexelSize.w);
				float2 pos = ((v.vertex.xy) * scale * 2.0);		

				// we're rendering with upside-down flipped projection,
				// so flip the vertical UV coordinate too
				if (_ProjectionParams.x < 0)
				{
					pos.y = (1 - pos.y) - 1;
				}

				o.vertex = float4(pos.xy, UNITY_NEAR_CLIP_VALUE, 1);

				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

				// Horrible hack to undo the scale transform to fit into our UV packing layout logic...
				if (_MainTex_ST.y < 0.0)
				{
					o.uv.y = 1.0 - o.uv.y;
				}

				o.uv = OffsetAlphaPackingUV(_MainTex_TexelSize, o.uv.xy, _MainTex_ST.y < 0.0);
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// Sample RGB
				fixed4 col = tex2D(_MainTex, i.uv.xy);
#if APPLY_GAMMA
				col.rgb = pow(col.rgb, 2.2);
#endif

#if ALPHAPACK_TOP_BOTTOM | ALPHAPACK_LEFT_RIGHT
				// Sample the alpha
				fixed4 alpha = tex2D(_MainTex, i.uv.zw);
#if APPLY_GAMMA
				alpha.rgb = pow(alpha.rgb, 2.2);
#endif
				col.a = (alpha.r + alpha.g + alpha.b) / 3.0;
#endif

				col *= _Color;

				return col;
			}
			ENDCG
		}
	}
}
