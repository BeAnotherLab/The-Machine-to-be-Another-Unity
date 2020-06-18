using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2020 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/Mesh Apply")]
	public class AVProLiveCameraMeshApply : MonoBehaviour
	{
		[SerializeField] AVProLiveCamera _liveCamera = null;
		[SerializeField] MeshRenderer _mesh = null;
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

		public MeshRenderer Mesh
		{
			get { return _mesh; }
			set
			{
				if (_mesh != value)
				{
					ApplyMapping(null);
					_mesh = value;
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
				if (_mesh != null)
				{
					if (_propTexture != -1)
					{
						Material[] materials = _mesh.materials;
						for (int i = 0; i < materials.Length; i++)
						{
							materials[i].SetTexture(_propTexture, texture);
						}
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