using UnityEngine;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraPixelBuffer
	{
		// Format conversion and texture output
		public Texture2D _texture;
		public int _innerWidth;
		public int _innerHeight;

		// Conversion params
		public int _width;
		public int _height;
		public TextureFormat _format;
		private int _deviceIndex;
		private int _bufferIndex;

		public AVProLiveCameraPixelBuffer(int deviceIndex, int bufferIndex)
		{
			_deviceIndex = deviceIndex;
			_bufferIndex = bufferIndex;
		}

		public bool Build(int width, int height, TextureFormat format = TextureFormat.RGBA32)
		{
			_width = width;
			_height = height;
			_format = format;

			if (CreateTexture())
			{
				AVProLiveCameraPlugin.SetTexturePointer(_deviceIndex, _bufferIndex, _texture.GetNativeTexturePtr());
				return true;
			}
			return false;
		}

		public void Close()
		{
			if (_texture != null)
			{
				Texture2D.Destroy(_texture);
				_texture = null;
			}
		}

		public bool RequiresTextureCrop()
		{
			bool result = false;
			if (_texture != null)
			{
				result = (_width != _texture.width || _height != _texture.height);
			}
			return result;
		}

		private bool CreateTexture()
		{
			// Calculate texture size
			int textureWidth = _width;
			int textureHeight = _height;
			_innerWidth = textureWidth;
			_innerHeight = textureHeight;

			// Unity 2019.1 no longer supports devices that require power-of-two textures
#if !UNITY_2019_1_OR_NEWER
			bool requiresPOT = (SystemInfo.npotSupport == NPOTSupport.None);

			// If the texture isn't a power of 2
			if (requiresPOT)
			{
				if (!Mathf.IsPowerOfTwo(_width) || !Mathf.IsPowerOfTwo(_height))
				{
					// We use a power-of-2 texture as Unity makes these internally anyway and not doing it seems to break things for texture updates
					textureWidth = Mathf.NextPowerOfTwo(textureWidth);
					textureHeight = Mathf.NextPowerOfTwo(textureHeight);
				}
			}
#endif

			// Create texture that stores the initial raw frame
			// If there is already a texture, only destroy it if it isn't of desired size
			if (_texture != null)
			{
				if (_texture.width != textureWidth ||
					_texture.height != textureHeight ||
					_texture.format != _format)
				{
					Texture2D.Destroy(_texture);
					_texture = null;
				}
			}
			if (_texture == null)
			{
				_texture = new Texture2D(textureWidth, textureHeight, _format, false, true);
				_texture.wrapMode = TextureWrapMode.Clamp;
				_texture.filterMode = FilterMode.Point;
				_texture.name = "AVProLiveCamera-BufferTexture";
				_texture.Apply(false, true);
			}

			return (_texture != null);
		}
	}
}