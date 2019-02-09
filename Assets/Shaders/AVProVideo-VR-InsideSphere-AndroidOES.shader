Shader "AVProVideo/VR/InsideSphere Unlit (stereo) - Android OES ONLY" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[KeywordEnum(None, Top_Bottom, Left_Right)] Stereo("Stereo Mode", Float) = 0
		[Toggle(STEREO_DEBUG)] _StereoDebug("Stereo Debug Tinting", Float) = 0
		[Toggle(APPLY_GAMMA)] _ApplyGamma("Apply Gamma", Float) = 0
	}
	SubShader 
	{
		Tags{ "Queue" = "Geometry" }
		Pass
		{ 
			//ZTest Always
			Cull Front
			ZWrite Off
			Lighting Off

			GLSLPROGRAM

			#pragma only_renderers gles gles3
			#pragma multi_compile MONOSCOPIC STEREO_TOP_BOTTOM STEREO_LEFT_RIGHT
			#pragma multi_compile __ STEREO_DEBUG

			#extension GL_OES_EGL_image_external : enable
			#extension GL_OES_EGL_image_external_essl3 : enable
			precision mediump float;

			#ifdef VERTEX

#include "UnityCG.glslinc"
#define SHADERLAB_GLSL
#include "AVProVideo.cginc"

		varying vec2 texVal;
		uniform vec3 _cameraPosition;
		uniform mat4 _ViewMatrix;

#if defined(STEREO_DEBUG)
		varying vec4 tint;
#endif

			void main()
			{
				gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
				texVal = gl_MultiTexCoord0.xy;

				texVal.x = 1.0 - texVal.x;
				texVal.y = 1.0 - texVal.y;

#if defined(STEREO_TOP_BOTTOM) | defined(STEREO_LEFT_RIGHT)
				bool isLeftEye = IsStereoEyeLeft(_cameraPosition, _ViewMatrix[0].xyz);

				vec4 scaleOffset = GetStereoScaleOffset(isLeftEye);

				texVal.xy *= scaleOffset.xy;
				texVal.xy += scaleOffset.zw;

#endif			
#if defined(STEREO_DEBUG)
				tint = GetStereoDebugTint(IsStereoEyeLeft(_cameraPosition, _ViewMatrix[0].xyz));
#endif
            }
            #endif  

			#ifdef FRAGMENT

			varying vec2 texVal;
#if defined(STEREO_DEBUG)
			varying vec4 tint;
#endif

			uniform samplerExternalOES _MainTex;

            void main()
            {          
#if defined(SHADER_API_GLES) | defined(SHADER_API_GLES3)
				gl_FragColor = texture2D(_MainTex, texVal.xy);
#else
				gl_FragColor = vec4(1.0, 1.0, 0.0, 1.0);
#endif

#if defined(STEREO_DEBUG)
				gl_FragColor *= tint;
#endif
			}
            #endif       
				
			ENDGLSL
		}
	}
	
	Fallback "AVProVideo/VR/InsideSphere Unlit (stereo+fog)"
}