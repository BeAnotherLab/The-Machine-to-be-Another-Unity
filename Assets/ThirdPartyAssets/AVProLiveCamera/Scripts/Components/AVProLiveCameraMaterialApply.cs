using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2022 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/Material Apply")]
	public class AVProLiveCameraMaterialApply : MonoBehaviour
	{
		[SerializeField] AVProLiveCamera _liveCamera = null;
		[SerializeField] Material _material = null;
		[SerializeField] string _texturePropertyName = "_MainTex";

		private int _propTexture = -1;
		private Texture _lastTexture;

		public AVProLiveCamera LiveCamera
		{
			get { return _liveCamera; }
			set
			{
				if (_liveCamera != value)
				{
					_liveCamera = value;
					Update();
				}
			}
		}

		public Material Material
		{
			get { return _material; }
			set
			{
				if (_material != value)
				{
					ApplyMapping(null);
					_material = value;
					Update();
				}
			}
		}

		public string TexturePropertyName
		{
			get { return _texturePropertyName; }
			set
			{
				if (_texturePropertyName != value)
				{
					ApplyMapping(null);
					_texturePropertyName = value;
					_propTexture = Shader.PropertyToID(_texturePropertyName);
					Update();
				}
			}
		}

		void Awake()
		{
			_propTexture = Shader.PropertyToID(_texturePropertyName);
		}
		
		void Update()
		{
			if (_liveCamera != null && _liveCamera.OutputTexture != null)
			{
				ApplyMapping(_liveCamera.OutputTexture);
			}
			else
			{
				ApplyMapping(null);
			}
		}

		void ApplyMapping(Texture texture)
		{
			if (_lastTexture != texture)
			{
				if (_material != null)
				{
					if (_propTexture != -1)
					{
						if (!_material.HasProperty(_propTexture))
						{
							Debug.LogError(string.Format("[AVProLiveCamera] Material {0} doesn't have texture property {1}", _material.name, _texturePropertyName), this);
						}
						_material.SetTexture(_propTexture, texture);
						_lastTexture = texture;
					}
				}
			}
		}
		
		void OnDisable()
		{
			ApplyMapping(null);
		}
	}
}