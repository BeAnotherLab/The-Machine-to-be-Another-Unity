using UnityEngine;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2014-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	#if NGUI
	[AddComponentMenu("AVPro Live Camera/Apply to NGUI UITexture")]
	public class AVProLiveCameraApplyUITextureNGUI : MonoBehaviour 
	{
		public UITexture _uiTexture;
		public AVProLiveCamera _liveCamera;
		public Texture2D _defaultTexture;
		private AVProLiveCamera _currentCamera;
		private static Texture2D _blackTexture;
		[SerializeField] bool _makePixelPerfect = false;
	
		void Awake()
		{
			if (_blackTexture == null)
				CreateTexture();
		}

		void Start()
		{
			if (_defaultTexture == null)
			{
				_defaultTexture = _blackTexture;
			}
		
			Update();
		}

		void Update()
		{
			if (_liveCamera != null)
			{
				if (_liveCamera.OutputTexture != null)
				{
					_currentCamera = _liveCamera;

					if (_makePixelPerfect)
					{
						_currentCamera.OutputTexture.filterMode = FilterMode.Point;
						_uiTexture.mainTexture = _currentCamera.OutputTexture;
						_uiTexture.MakePixelPerfect();
					}
					else
					{
						_uiTexture.mainTexture = _currentCamera.OutputTexture;
					}
				}
			}
			else
			{	
				_currentCamera = null;
				_uiTexture.mainTexture = _defaultTexture;
			}
		}
	
		public void OnDisable()
		{
			//_uiTexture.mainTexture = null;
			//_currentCamera = null;
		}

		void OnDestroy()
		{
			_defaultTexture = null;
		
			if (_blackTexture != null)
			{
				Texture2D.Destroy(_blackTexture);
				_blackTexture = null;
			}

			_uiTexture.mainTexture = null;
		}

		private static void CreateTexture()
		{
			_blackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false, false);
			_blackTexture.name = "AVProLiveCamera-BlackTexture";
			_blackTexture.filterMode = FilterMode.Point;
			_blackTexture.wrapMode = TextureWrapMode.Clamp;
			_blackTexture.SetPixel(0, 0, Color.black);
			_blackTexture.Apply(false, true);
		}
	}
	#endif
}