// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AVProVideo/VR/InsideSphere Unlit (stereo+fog)"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo ("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug ("Stereo Debug Tinting", Float) = 0
		[Toggle(HIGH_QUALITY)] _HighQuality ("High Quality", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
    }
    SubShader
    {
		Tags { "RenderType"="Opaque" "IgnoreProjector" = "True" "Queue" = "Background" }
		ZWrite On
		//ZTest Always
		Cull Front
		Lighting Off

        Pass
        {
            CGPROGRAM
			#include "UnityCG.cginc"
			#include "AVProVideo.cginc"
			//#pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

			//#define STEREO_DEBUG 1
			//#define HIGH_QUALITY 1

			#pragma multi_compile_fog
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
			#pragma multi_compile __ STEREO_DEBUG
			#pragma multi_compile __ HIGH_QUALITY
			#pragma multi_compile __ APPLY_GAMMA

            struct appdata
            {
                float4 vertex : POSITION; // vertex position
#if HIGH_QUALITY
				float3 normal : NORMAL;
#else
                float2 uv : TEXCOORD0; // texture coordinate			
#endif
				
            };

            struct v2f
            {
                float4 vertex : SV_POSITION; // clip space position
#if HIGH_QUALITY
				float3 normal : TEXCOORD0;
				
#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
				float4 scaleOffset : TEXCOORD1; // texture coordinate
#if UNITY_VERSION >= 500
				UNITY_FOG_COORDS(2)
#endif
#else
#if UNITY_VERSION >= 500
				UNITY_FOG_COORDS(1)
#endif
#endif
#else
                float2 uv : TEXCOORD0; // texture coordinate

#if UNITY_VERSION >= 500
				UNITY_FOG_COORDS(1)
#endif
#endif

#if STEREO_DEBUG
				float4 tint : COLOR;
#endif
            };

            uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float3 _cameraPosition;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
#if !HIGH_QUALITY
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.xy = float2(1.0-o.uv.x, o.uv.y);
#endif

#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
				float4 scaleOffset = GetStereoScaleOffset(IsStereoEyeLeft(_cameraPosition, UNITY_MATRIX_V[0].xyz));

				#if !HIGH_QUALITY
				o.uv.xy *= scaleOffset.xy;
				o.uv.xy += scaleOffset.zw;
				#else
				o.scaleOffset = scaleOffset;
				#endif
#endif

#if HIGH_QUALITY
				o.normal = v.normal;
#endif

				#if STEREO_DEBUG
				o.tint = GetStereoDebugTint(IsStereoEyeLeft(_cameraPosition, UNITY_MATRIX_V[0].xyz));
				#endif

#if UNITY_VERSION >= 500
				UNITY_TRANSFER_FOG(o, o.vertex);
#endif

                return o;
			}

            
            fixed4 frag (v2f i) : SV_Target
            {
				float2 uv;

#if HIGH_QUALITY
				float3 n = normalize(i.normal);

				float M_1_PI = 1.0 / 3.1415926535897932384626433832795;
				float M_1_2PI = 1.0 / 6.283185307179586476925286766559;
				uv.x = 0.5 - atan2(n.z, n.x) * M_1_2PI;
				uv.y = 0.5 - asin(-n.y) * M_1_PI;
				uv.x += 0.75;
				uv.x = fmod(uv.x, 1.0);
				//uv.x = uv.x % 1.0;
				uv.xy = TRANSFORM_TEX(uv, _MainTex);
				#if STEREO_TOP_BOTTOM | STEREO_LEFT_RIGHT
				uv.xy *= i.scaleOffset.xy;
				uv.xy += i.scaleOffset.zw;
				#endif
#else
				uv = i.uv;
#endif

                fixed4 col = tex2D(_MainTex, uv);

#if APPLY_GAMMA
				col.rgb = pow(col.rgb, 2.2);
#endif

#if STEREO_DEBUG
				col *= i.tint;
#endif

#if UNITY_VERSION >= 500
				UNITY_APPLY_FOG(i.fogCoord, col);
#endif

                return fixed4(col.rgb, 1.0);
            }
            ENDCG
        }
    }
}