//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraDeviceMode
	{
		private AVProLiveCameraDevice _device;
		private int _internalIndex;
		private int _width, _height;
		private float _fps;
		private string _format;

		public int Width
		{
			get { return _width; }
		}

		public int Height
		{
			get { return _height; }
		}

		public float FPS
		{
			get { return _fps; }
		}

		public string Format
		{
			get { return _format; }
		}

		public int InternalIndex
		{
			get { return _internalIndex; }
		}

		public AVProLiveCameraDevice Device
		{
			get { return _device; }
		}

		public AVProLiveCameraDeviceMode(AVProLiveCameraDevice device, int internalIndex, int width, int height, float fps, string format)
		{
			_device = device;
			_internalIndex = internalIndex;
			_width = width;
			_height = height;
			_fps = fps;
			_format = format;
		}
	}
}