#define TEXTURETEST
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2014-2018 RenderHeads Ltd.  All rights reserverd.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	[AddComponentMenu("AVPro Live Camera/Grabber")]
	public class AVProLiveCameraGrabber : MonoBehaviour
	{
		public AVProLiveCamera _camera;
		private AVProLiveCameraDevice _device;

		private Color32[] _frameData;
		private int _frameWidth;
		private int _frameHeight;
		private GCHandle _frameHandle;
		private System.IntPtr _framePointer;
		private uint _lastFrame;

#if TEXTURETEST
		private Texture2D _testTexture;
#endif

		void Update()
		{
			if (_camera != null)
				_device = _camera.Device;

			if (_device != null && _device.IsActive && !_device.IsPaused)
			{
				if (_device.CurrentWidth > _frameWidth ||
					_device.CurrentHeight > _frameHeight)
				{
					CreateBuffer(_device.CurrentWidth, _device.CurrentHeight);
				}
				uint lastFrame = AVProLiveCameraPlugin.GetLastFrame(_device.DeviceIndex);
				if (lastFrame != _lastFrame)
				{
					_lastFrame = lastFrame;
					bool result = AVProLiveCameraPlugin.GetFrameAsColor32(_device.DeviceIndex, _framePointer, _frameWidth, _frameHeight);
					if (result)
					{
#if TEXTURETEST
						_testTexture.SetPixels32(_frameData);
						_testTexture.Apply(false, false);
#endif
					}
				}
			}
		}

		private void CreateBuffer(int width, int height)
		{
			// Free buffer if it's too small
			if (_frameHandle.IsAllocated && _frameData != null)
			{
				if (_frameData.Length < _frameWidth * _frameHeight)
				{
					FreeBuffer();
				}
			}

			if (_frameData == null)
			{
				_frameWidth = width;
				_frameHeight = height;
				_frameData = new Color32[_frameWidth * _frameHeight];
				_frameHandle = GCHandle.Alloc(_frameData, GCHandleType.Pinned);
				_framePointer = _frameHandle.AddrOfPinnedObject();

#if TEXTURETEST
				_testTexture = new Texture2D(_frameWidth, _frameHeight, TextureFormat.ARGB32, false, false);
				_testTexture.Apply(false, false);
#endif
			}
		}

		private void FreeBuffer()
		{
			if (_frameHandle.IsAllocated)
			{
				_framePointer = System.IntPtr.Zero;
				_frameHandle.Free();
				_frameData = null;
			}

#if TEXTURETEST
			if (_testTexture)
			{
				Texture2D.DestroyImmediate(_testTexture);
				_testTexture = null;
			}
#endif
		}

		void OnDestroy()
		{
			FreeBuffer();
		}

#if TEXTURETEST
		void OnGUI()
		{
			if (_testTexture)
			{
				GUI.depth = 1;
				GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _testTexture, ScaleMode.ScaleToFit, false);
			}
		}
#endif
	}
}