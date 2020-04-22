using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/IMGUI Display")]
	public class AVProLiveCameraGUIDisplay : MonoBehaviour
	{
		public AVProLiveCamera _liveCamera;

		public ScaleMode _scaleMode = ScaleMode.ScaleToFit;
		public Color _color = Color.white;
		public int _depth = 0;

		public bool _fullScreen = true;
		public float _x = 0.0f;
		public float _y = 0.0f;
		public float _width = 1.0f;
		public float _height = 1.0f;

		public bool _flipX;
		public bool _flipY;

		private static int _propApplyGamma;
		private static int _propFlip;
		private static Shader _shaderGammaConversion;
		private static Shader _shaderGammaConversionTransparent;
		private Material _material;

		//-------------------------------------------------------------------------

		void Awake()
		{
			if (_propApplyGamma == 0)
			{
				_propApplyGamma = Shader.PropertyToID("_ApplyGamma");
				_propFlip = Shader.PropertyToID("_Flip");
			}
		}

		void Start()
		{
			// Disabling this lets you skip the GUI layout phase.
			this.useGUILayout = false;

			if (_shaderGammaConversion == null)
			{
				_shaderGammaConversion = Shader.Find("Hidden/AVProLiveCamera/IMGUI");
			}
			if (_shaderGammaConversionTransparent == null)
			{
				_shaderGammaConversionTransparent = Shader.Find("Hidden/AVProLiveCamera/IMGUI Transparent");
			}
		}

		void OnDestroy()
		{
			// Destroy existing material
			if (_material != null)
			{
#if UNITY_EDITOR
				Material.DestroyImmediate(_material);
#else
				Material.Destroy(_material);
#endif
				_material = null;
			}
		}

		private Shader GetRequiredShader()
		{
			Shader result = null;

			if (QualitySettings.activeColorSpace == ColorSpace.Linear)
			{
				if (_liveCamera != null && _liveCamera.Device != null && _liveCamera.Device.SupportsTransparency)
				{
					result = _shaderGammaConversionTransparent;
				}
				else
				{
					result = _shaderGammaConversion;
				}
			}

			return result;
		}


		void Update()
		{
			// Get required shader
			Shader currentShader = null;
			if (_material != null)
			{
				currentShader = _material.shader;
			}
			Shader nextShader = GetRequiredShader();

			// If the shader requirement has changed
			if (currentShader != nextShader)
			{
				// Destroy existing material
				if (_material != null)
				{
#if UNITY_EDITOR
					Material.DestroyImmediate(_material);
#else
					Material.Destroy(_material);
#endif
					_material = null;
				}

				// Create new material
				if (nextShader != null)
				{
					_material = new Material(nextShader);
					if (_material.HasProperty(_propApplyGamma))
					{
						if (QualitySettings.activeColorSpace == ColorSpace.Linear)
						{
							_material.EnableKeyword("APPLY_GAMMA");
						}
						else
						{
							_material.DisableKeyword("APPLY_GAMMA");
						}
					}
				}
			}
		}

		public void OnGUI()
		{
			if (_liveCamera == null)
				return;

			_x = Mathf.Clamp01(_x);
			_y = Mathf.Clamp01(_y);
			_width = Mathf.Clamp01(_width);
			_height = Mathf.Clamp01(_height);

			if (_liveCamera.OutputTexture != null)
			{
				GUI.depth = _depth;
				GUI.color = _color;

				Rect rect;
				if (_fullScreen)
					rect = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
				else
					rect = new Rect(_x * (Screen.width - 1), _y * (Screen.height - 1), _width * Screen.width, _height * Screen.height);

				if (_material != null)
				{
					Vector2 flip = Vector2.one;
					if (_flipX)
					{
						flip.x = -1f;
					}
					if (_flipY)
					{
						flip.y = -1f;
					}
					_material.SetVector(_propFlip, flip);
					DrawTexture(rect, _liveCamera.OutputTexture, _scaleMode, _material);
				}
				else
				{
					if (_flipX || _flipY)
					{
						Vector2 pivot = new Vector2(rect.x + (rect.width / 2), rect.y + (rect.height / 2));
						Vector2 scale = Vector2.one;
						if (_flipX)
							scale.x = -1.0f;
						if (_flipY)
							scale.y = -1.0f;
						GUIUtility.ScaleAroundPivot(scale, pivot);
					}

					GUI.DrawTexture(rect, _liveCamera.OutputTexture, _scaleMode, _liveCamera.Device.SupportsTransparency);
				}
			}
		}

		private static void DrawTexture(Rect screenRect, Texture texture, ScaleMode scaleMode, Material material)
		{
			if (Event.current.type == EventType.Repaint)
			{
				float textureWidth = texture.width;
				float textureHeight = texture.height;

				float aspectRatio = (float)textureWidth / (float)textureHeight;
				Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
				switch (scaleMode)
				{
					case ScaleMode.ScaleAndCrop:
						{
							float screenRatio = screenRect.width / screenRect.height;
							if (screenRatio > aspectRatio)
							{
								float adjust = aspectRatio / screenRatio;
								sourceRect = new Rect(0f, (1f - adjust) * 0.5f, 1f, adjust);
							}
							else
							{
								float adjust = screenRatio / aspectRatio;
								sourceRect = new Rect(0.5f - adjust * 0.5f, 0f, adjust, 1f);
							}
						}
						break;
					case ScaleMode.ScaleToFit:
						{
							float screenRatio = screenRect.width / screenRect.height;
							if (screenRatio > aspectRatio)
							{
								float adjust = aspectRatio / screenRatio;
								screenRect = new Rect(screenRect.xMin + screenRect.width * (1f - adjust) * 0.5f, screenRect.yMin, adjust * screenRect.width, screenRect.height);
							}
							else
							{
								float adjust = screenRatio / aspectRatio;
								screenRect = new Rect(screenRect.xMin, screenRect.yMin + screenRect.height * (1f - adjust) * 0.5f, screenRect.width, adjust * screenRect.height);
							}
						}
						break;
					case ScaleMode.StretchToFill:
						break;
				}
				Graphics.DrawTexture(screenRect, texture, sourceRect, 0, 0, 0, 0, GUI.color, material);
			}
		}
	}
}