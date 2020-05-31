Shader "WebRtcVideoChat/wrtcI420"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_UPlane("UPlane", 2D) = "white" {}
		_VPlane("VPlane", 2D) = "white" {}
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType"="Opaque"
		}

		Pass
		{
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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

			sampler2D _MainTex;
			sampler2D _UPlane;
			sampler2D _VPlane;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				fixed y = tex2D(_MainTex, i.uv);
				fixed u = tex2D(_UPlane, i.uv);
				fixed v = tex2D(_VPlane, i.uv);

				u = u - 0.5;
				v = v - 0.5;
				fixed r = (1.164f * y + 1.596f * v);
				fixed g = (1.164f * y - 0.813f * v - 0.391f * u);
				fixed b = (1.164f * y + 2.018f * u);

				return fixed4(r, g, b, 1);

			}
			ENDCG
		}
	}
}
