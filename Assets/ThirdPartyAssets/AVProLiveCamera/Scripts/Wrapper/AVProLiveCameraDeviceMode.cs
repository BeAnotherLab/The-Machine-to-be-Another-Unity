//-----------------------------------------------------------------------------
// Copyright 2012-2022 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraDeviceMode
	{
		private AVProLiveCameraDevice _device;
		private int _internalIndex;
		private int _width, _height;
		private int _frameRateIndex;
		private float[] _frameRates;
		private string _format;

		public int Width
		{
			get { return _width; }
		}

		public int Height
		{
			get { return _height; }
		}

		public float[] FrameRates
		{
			get { return _frameRates; }
		}

		public int FrameRateIndex
		{
			get { return _frameRateIndex; }
			set { if (value >= 0 && value < _frameRates.Length) _frameRateIndex = value; }
		}

		public float FPS
		{
			get { return _frameRates[_frameRateIndex]; }
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

		public void SelectHighestFrameRate()
		{
			_frameRateIndex = 0;
			for (int i = 0; i < _frameRates.Length; i++)
			{
				if (_frameRates[i] > FPS)
				{
					_frameRateIndex = i;
				}
			}
		}

		internal bool HasFrameRate(float frameRate)
		{
			bool result = false;
			for (int i = 0; i < _frameRates.Length; i++)
			{
				if (_frameRates[i] == frameRate)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		internal float HighestFrameRate()
		{
			float highestFps = 0f;
			for (int i = 0; i < _frameRates.Length; i++)
			{
				if (_frameRates[i] > highestFps)
				{
					highestFps = _frameRates[i];
				}
			}
			return highestFps;
		}

		internal float GetClosestFrameRate(float frameRate)
		{
			float result = float.MinValue;
			float lowestDelta = 10000f;
			for (int i = 0; i < _frameRates.Length; i++)
			{
				float d = UnityEngine.Mathf.Abs(_frameRates[i] - frameRate);
				if (d < lowestDelta)
				{
					result = _frameRates[i];
					lowestDelta = d;
				}
			}
			return result;
		}

		public void SelectClosestFrameRate(float frameRate)
		{
			float lowestDelta = 10000f;
			for (int i = 0; i < _frameRates.Length; i++)
			{
				float d = UnityEngine.Mathf.Abs(_frameRates[i] - frameRate);
				if (d < lowestDelta)
				{
					_frameRateIndex = i;
					lowestDelta = d;
				}
			}
		}

		public AVProLiveCameraDeviceMode(AVProLiveCameraDevice device, int internalIndex, int width, int height, float[] frameRates, int defaultFrameRateIndex, string format)
		{
			_device = device;
			_internalIndex = internalIndex;
			_width = width;
			_height = height;
			_frameRates = frameRates;
			_frameRateIndex = defaultFrameRateIndex;
			_format = format;
		}
	}
}