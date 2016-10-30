Shader "AVProVideo/Lit/Diffuse (texture+color+fog+stereo support)" 
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}

		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}

	SubShader
	{
		Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Geometry" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:VertexFunction
		#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
		// Note: Ideally "__" should be used here, but causes Unity 4.6.8 compile errors
		#pragma multi_compile STEREO_DEBUG_OFF STEREO_DEBUG			
		#pragma multi_compile __ APPLY_GAMMA

		#include "AVProVideo.cginc"

		uniform sampler2D _MainTex;
		uniform fixed4 _Color;
		uniform float3 _cameraPosition;

		struct Input 
		{
			float2 uv_MainTex;
#if STEREO_DEBUG
			float4 color;
#endif
		};

		void VertexFunction(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
			float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(_cameraPosition, UNITY_MATRIX_V[0].xyz));
			o.uv_MainTex = v.texcoord.xy *= scaleOffset.xy;
			o.uv_MainTex = v.texcoord.xy += scaleOffset.zw;
#endif
#if STEREO_DEBUG
			o.color = GetStereoDebugTint(IsStereoEyeLeft(_cameraPosition, UNITY_MATRIX_V[0].xyz));
#endif
		}

		void surf(Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
#if APPLY_GAMMA
			c.rgb = pow(c.rgb, 2.2);
#endif			
#if STEREO_DEBUG
			c *= IN.color;
#endif
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Legacy Shaders/Transparent/VertexLit"
}