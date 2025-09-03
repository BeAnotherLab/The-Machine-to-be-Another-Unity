using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraFormatConverter : System.IDisposable
	{
		private int _deviceIndex;

		// Format conversion and texture output
		private RenderTexture _finalTexture;
		private Material _conversionMaterial;
		private Material _deinterlaceMaterial;

		// Conversion params
		private int _width;
		private int _height;
		private bool _flipX;
		private bool _flipY;
		private bool _allowTransparency;
		private bool _deinterlace;
		private AVProLiveCameraPlugin.VideoFrameFormat _format;

		private List<AVProLiveCameraPixelBuffer> _buffers;
		private Vector4[] _uvs;
		private bool _requiresTextureCrop;
		private int _lastFrameUpdated = 0;
		//private AVProLiveCameraManager _manager;

		public Texture OutputTexture
		{
			get { return _finalTexture; }
		}

		public bool FlipX
		{
			get { return _flipX; }
			set { if (_flipX != value) SetFlip(value, _flipY); }
		}

		public bool FlipY
		{
			get { return _flipY; }
			set { if (_flipY != value) SetFlip(_flipX, value); }
		}

		public bool AllowTransparency
		{
			get { return _allowTransparency; }
			set { if (_allowTransparency != value) SetTransparency(value); }
		}

		public bool ValidPicture { get; private set; }

		public AVProLiveCameraFormatConverter(int deviceIndex)
		{
			ValidPicture = false;
			_deviceIndex = deviceIndex;
			_buffers = new List<AVProLiveCameraPixelBuffer>(4);
		}

		public void Reset()
		{
			ValidPicture = false;
			_lastFrameUpdated = 0;
		}

		public bool Build(int width, int height, AVProLiveCameraPlugin.VideoFrameFormat format, bool allowTransparency, bool flipX, bool flipY, bool deinterlace = false)
		{
			Reset();

			_width = width;
			_height = height;
			_deinterlace = deinterlace;
			_format = format;
			_flipX = flipX;
			_flipY = flipY;
			//_manager = AVProLiveCameraManager.Instance;

			if (CreateMaterials())
			{
				CreateBuffers();
				CreateRenderTexture();

				switch (_format)
				{
					case AVProLiveCameraPlugin.VideoFrameFormat.RAW_BGRA32:
						_conversionMaterial.SetTexture("_MainTex", _buffers[0]._texture);
						break;
					case AVProLiveCameraPlugin.VideoFrameFormat.RAW_MONO8:
						_conversionMaterial.SetFloat("_TextureWidth", _finalTexture.width);
						_conversionMaterial.SetTexture("_MainTex", _buffers[0]._texture);
						break;
					case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_YUY2:
					case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_UYVY:
					case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_YVYU:
					case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_HDYC:
						_conversionMaterial.SetFloat("_TextureWidth", _finalTexture.width);
						_conversionMaterial.SetTexture("_MainTex", _buffers[0]._texture);
						break;
					case AVProLiveCameraPlugin.VideoFrameFormat.YUV_420_PLANAR_YV12:
					case AVProLiveCameraPlugin.VideoFrameFormat.YUV_420_PLANAR_I420:
						_conversionMaterial.SetFloat("_TextureWidth", _finalTexture.width);
						_conversionMaterial.SetTexture("_MainTex", _buffers[0]._texture);
						_conversionMaterial.SetTexture("_MainU", _buffers[1]._texture);
						_conversionMaterial.SetTexture("_MainV", _buffers[2]._texture);
						break;
				}

				SetFlip(_flipX, _flipY);
				SetTransparency(allowTransparency);
			}
			else
			{
				Debug.LogWarning("[AVPro LiveCamera] couldn't create conversion materials");
				return false;
			}

			return true;
		}

		private bool IsNewSourceTextureAvailable()
		{
			bool result = false;

			int lastFrameUpdated = AVProLiveCameraPlugin.GetLastFrameUploaded(_deviceIndex);
			if (_lastFrameUpdated != lastFrameUpdated)
			{
				_lastFrameUpdated = lastFrameUpdated;
				result = true;
			}
			return result;
		}

		public bool Update()
		{
			//RenderTexture prev = RenderTexture.active;
			bool isTextureUpdated = IsNewSourceTextureAvailable();

			if (isTextureUpdated)
			{
				if (!_deinterlaceMaterial)
				{
					// Format convert
					if (DoFormatConversion(_finalTexture))
					{
						ValidPicture = true;
					}
				}
				else
				{
					// Format convert and Deinterlace
					RenderTexture tempTarget = RenderTexture.GetTemporary(_width, _height, 0, _finalTexture.format);
					if (DoFormatConversion(tempTarget))
					{
						DoDeinterlace(tempTarget, _finalTexture);
						ValidPicture = true;
					}
					RenderTexture.ReleaseTemporary(tempTarget);
				}
			}
			//RenderTexture.active = prev;		

			return (ValidPicture && isTextureUpdated);
		}

		public void Dispose()
		{
			ValidPicture = false;

			if (_conversionMaterial != null)
			{
				_conversionMaterial.mainTexture = null;
				Material.Destroy(_conversionMaterial);
				_conversionMaterial = null;
			}

			if (_deinterlaceMaterial != null)
			{
				_deinterlaceMaterial.mainTexture = null;
				Material.Destroy(_deinterlaceMaterial);
				_deinterlaceMaterial = null;
			}

			if (_finalTexture != null)
			{
				RenderTexture.ReleaseTemporary(_finalTexture);
				_finalTexture = null;
			}

			foreach (AVProLiveCameraPixelBuffer buffer in _buffers)
			{
				buffer.Close();
			}
			_buffers.Clear();
		}

		private void CreateBuffers()
		{
			foreach (AVProLiveCameraPixelBuffer buffer in _buffers)
			{
				buffer.Close();
			}
			_buffers.Clear();

			_requiresTextureCrop = false;
			switch (_format)
			{
				case AVProLiveCameraPlugin.VideoFrameFormat.RAW_BGRA32:
					if (_buffers.Count < 1)
						_buffers.Add(new AVProLiveCameraPixelBuffer(_deviceIndex, _buffers.Count));
					_buffers[0].Build(_width, _height);
					_requiresTextureCrop = _buffers[0].RequiresTextureCrop();
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.RAW_MONO8:
					if (_buffers.Count < 1)
						_buffers.Add(new AVProLiveCameraPixelBuffer(_deviceIndex, _buffers.Count));
					_buffers[0].Build(_width / 4, _height);
					_requiresTextureCrop = _buffers[0].RequiresTextureCrop();
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_YUY2:
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_UYVY:
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_YVYU:
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_HDYC:
					if (_buffers.Count < 1)
						_buffers.Add(new AVProLiveCameraPixelBuffer(_deviceIndex, _buffers.Count));
					_buffers[0].Build(_width / 2, _height);     // YCbCr422 modes need half width
					_requiresTextureCrop = _buffers[0].RequiresTextureCrop();
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_420_PLANAR_YV12:
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_420_PLANAR_I420:
					while (_buffers.Count < 3)
					{
						_buffers.Add(new AVProLiveCameraPixelBuffer(_deviceIndex, _buffers.Count));
					}
					_buffers[0].Build(_width / 4, _height);         // Y
					_buffers[1].Build(_width / 8, _height / 2);     // Cr
					_buffers[2].Build(_width / 8, _height / 2);     // Cb
					_requiresTextureCrop = true;
					break;
			}
		}

		private bool CreateMaterials()
		{
			Shader shader = AVProLiveCameraManager.Instance.GetPixelConversionShader(_format);
			if (shader)
			{
				if (_conversionMaterial != null)
				{
					if (_conversionMaterial.shader != shader)
					{
						Material.Destroy(_conversionMaterial);
						_conversionMaterial = null;
					}
				}

				if (_conversionMaterial == null)
				{
					_conversionMaterial = new Material(shader);
					_conversionMaterial.name = "AVProLiveCamera-Material";

					// Since Unity 5.3 Graphics.Blit ignores mainTextureOffset/Scale
#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2)
					_conversionMaterial.SetVector("_MainTex_ST2", new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
#endif
				}
			}

			if (_deinterlace)
			{
				shader = AVProLiveCameraManager.Instance.GetDeinterlaceShader();
				if (shader)
				{
					if (_deinterlaceMaterial != null)
					{
						if (_deinterlaceMaterial.shader != shader)
						{
							Material.Destroy(_deinterlaceMaterial);
							_deinterlaceMaterial = null;
						}
					}

					if (_deinterlaceMaterial == null)
					{
						_deinterlaceMaterial = new Material(shader);
						_deinterlaceMaterial.name = "AVProLiveCamera-DeinterlaceMaterial";
					}
				}
			}

			return (_conversionMaterial != null) && (!_deinterlace || _deinterlaceMaterial != null);
		}

		private void CreateRenderTexture()
		{
			// Create RenderTexture for post transformed frames
			// If there is already a renderTexture, only destroy it smaller than desired size
			if (_finalTexture != null)
			{
				if (_finalTexture.width != _width || _finalTexture.height != _height)
				{
					RenderTexture.ReleaseTemporary(_finalTexture);
					_finalTexture = null;
				}
			}
			if (_finalTexture == null)
			{
				ValidPicture = false;
				RenderTextureReadWrite rwMode = RenderTextureReadWrite.sRGB;
#if UNITY_2018_1_OR_NEWER
				rwMode = RenderTextureReadWrite.Linear;
#endif
				_finalTexture = RenderTexture.GetTemporary(_width, _height, 0, RenderTextureFormat.ARGB32, rwMode);
				_finalTexture.wrapMode = TextureWrapMode.Clamp;
				_finalTexture.filterMode = FilterMode.Bilinear;
				_finalTexture.name = "AVProLiveCamera-FinalTexture";
				_finalTexture.Create();
			}
		}

		private bool DoFormatConversion(RenderTexture target)
		{
			if (_buffers == null || _buffers.Count == 0)
				return false;

			target.DiscardContents();

			if (!_requiresTextureCrop)
			{
				RenderTexture prev = RenderTexture.active;
				Graphics.Blit(_buffers[0]._texture, target, _conversionMaterial, 0);
				RenderTexture.active = prev;
			}
			else
			{
				RenderTexture prev = RenderTexture.active;
				RenderTexture.active = target;
				_conversionMaterial.SetPass(0);

				GL.PushMatrix();
				GL.LoadOrtho();
				DrawQuad();
				GL.PopMatrix();

				RenderTexture.active = prev;
			}

			return true;
		}

		private void DoDeinterlace(RenderTexture source, RenderTexture target)
		{
			target.DiscardContents();
			RenderTexture prev = RenderTexture.active;
			Graphics.Blit(source, target, _deinterlaceMaterial);
			RenderTexture.active = prev;
		}

		private void SetFlip(bool flipX, bool flipY)
		{
			_flipX = flipX;
			_flipY = flipY;

			if (_requiresTextureCrop)
			{
				if (_buffers != null)
				{
					BuildUVs(_flipX, _flipY);
				}
			}
			else
			{
				if (_conversionMaterial != null)
				{
					// Flip and then offset back to get back to normalised range
					Vector2 scale = new Vector2(1f, 1f);
					Vector2 offset = new Vector2(0f, 0f);
					if (_flipX)
					{
						scale = new Vector2(-1f, scale.y);
						offset = new Vector2(1f, offset.y);
					}
					if (_flipY)
					{
						scale = new Vector2(scale.x, -1f);
						offset = new Vector2(offset.x, 1f);
					}

					_conversionMaterial.mainTextureScale = scale;
					_conversionMaterial.mainTextureOffset = offset;
					// Since Unity 5.3 Graphics.Blit ignores mainTextureOffset/Scale
#if UNITY_5_4_OR_NEWER || (UNITY_5 && !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2)
					_conversionMaterial.SetVector("_MainTex_ST2", new Vector4(scale.x, scale.y, offset.x, offset.y));
#endif

					// Horizontal flipping requires changes to how YUV pixels are read
					if (_flipX)
					{
						_conversionMaterial.DisableKeyword("HORIZONTAL_FLIP_OFF");
						_conversionMaterial.EnableKeyword("HORIZONTAL_FLIP_ON");
					}
					else
					{
						_conversionMaterial.DisableKeyword("HORIZONTAL_FLIP_ON");
						_conversionMaterial.EnableKeyword("HORIZONTAL_FLIP_OFF");
					}
				}
			}
		}

		private void SetTransparency(bool enabled)
		{
			_allowTransparency = enabled;
			if (_conversionMaterial != null)
			{
				if (enabled)
				{
					_conversionMaterial.DisableKeyword("AVPRO_TRANSPARENCY_OFF");
					_conversionMaterial.EnableKeyword("AVPRO_TRANSPARENCY_ON");
				}
				else
				{
					_conversionMaterial.DisableKeyword("AVPRO_TRANSPARENCY_ON");
					_conversionMaterial.EnableKeyword("AVPRO_TRANSPARENCY_OFF");
				}
			}
		}

		private void BuildUVs(bool invertX, bool invertY)
		{
			_uvs = new Vector4[_buffers.Count];

			for (int i = 0; i < _buffers.Count; i++)
			{
				float x1, x2;
				float y1, y2;
				if (invertX)
				{
					x1 = 1.0f; x2 = 0.0f;
				}
				else
				{
					x1 = 0.0f; x2 = 1.0f;
				}
				if (invertY)
				{
					y1 = 1.0f; y2 = 0.0f;
				}
				else
				{
					y1 = 0.0f; y2 = 1.0f;
				}

				// Alter UVs if we're only using a portion of the texture
				if (_buffers[i]._innerWidth != _buffers[i]._texture.width)
				{
					float xd = _buffers[i]._innerWidth / (float)_buffers[i]._texture.width;
					x1 *= xd; x2 *= xd;
				}
				if (_buffers[i]._innerHeight != _buffers[i]._texture.height)
				{
					float yd = _buffers[i]._innerHeight / (float)_buffers[i]._texture.height;
					y1 *= yd; y2 *= yd;
				}

				_uvs[i] = new Vector4(x1, y1, x2, y2);
			}
		}

		private void DrawQuad()
		{
			GL.Begin(GL.QUADS);

			for (int i = 0; i < _buffers.Count; i++)
				GL.MultiTexCoord2(i, _uvs[i].x, _uvs[i].y);
			GL.Vertex3(0.0f, 0.0f, 0.1f);

			for (int i = 0; i < _buffers.Count; i++)
				GL.MultiTexCoord2(i, _uvs[i].z, _uvs[i].y);
			GL.Vertex3(1.0f, 0.0f, 0.1f);

			for (int i = 0; i < _buffers.Count; i++)
				GL.MultiTexCoord2(i, _uvs[i].z, _uvs[i].w);
			GL.Vertex3(1.0f, 1.0f, 0.1f);

			for (int i = 0; i < _buffers.Count; i++)
				GL.MultiTexCoord2(i, _uvs[i].x, _uvs[i].w);
			GL.Vertex3(0.0f, 1.0f, 0.1f);

			GL.End();
		}
	}
}