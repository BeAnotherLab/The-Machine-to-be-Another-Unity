using UnityEngine;
using System.Text;
using System.Collections.Generic;

//-----------------------------------------------------------------------------
// Copyright 2012-2018 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

namespace RenderHeads.Media.AVProLiveCamera
{
	public class AVProLiveCameraDevice : System.IDisposable
	{
		private const int MaxVideoResolution = 16384;
		private int _deviceIndex;
		private List<AVProLiveCameraDeviceMode> _modes;
		private List<AVProLiveCameraSettingBase> _settings;
		private Dictionary<int, AVProLiveCameraSettingBase> _settingsByType;
		private List<string> _videoInputs;
		private AVProLiveCameraFormatConverter _formatConverter;
		private int _width;
		private int _height;
		private float _frameRate;
		private long _frameDurationHNS;
		private string _format;
		private string _deviceFormat;
		private int _frameCount;
		private float _startFrameTime;
		private int _lastModeIndex = -1;
		private int _lastVideoInputIndex = -1;
		private bool _isActive = false;
		private bool _isTopDown;
		private bool _flipX;
		private bool _flipY;
		private bool _allowTransparency = true;

		public enum SettingsEnum
		{
			// Video "proc amp" settings
			Brightness = 0,
			Contrast,
			Hue,
			Saturation,
			Sharpness,
			Gamma,
			ColorEnable,
			WhiteBalance,
			BacklightCompensation,
			Gain,
			DigitalMultiplier,
			DigitalMultiplierLimit,
			WhiteBalanceComponent,
			PowerlineFrequency,

			// Camera control settings
			Pan = 1000,
			Tilt,
			Roll,
			Zoom,
			Exposure,
			Iris,
			Focus,
			ScanMode,
			Privacy,
			PanTilt,
			PanRelative,
			TiltRelative,
			RollRelative,
			ZoomRelative,
			ExposureRelative,
			IrisRelative,
			FocusRelative,
			PanTiltRelative,
			FocalLength,
			AutoExposurePriority,

			// Logitech specific settings
			Logitech_Version = 2000,
			Logitech_DigitalPan,
			Logitech_DigitalTilt,
			Logitech_DigitalZoom,
			Logitech_DigitalPanTiltZoom,
			Logitech_ExposureTime,
			Logitech_FaceTracking,
			Logitech_LED,
			Logitech_FindFace,
		}

		public bool IsActive
		{
			get
			{
				return _isActive;
			}
			set
			{
				_isActive = value;
				if (_deviceIndex >= 0)
					AVProLiveCameraPlugin.SetActive(_deviceIndex, _isActive);
			}
		}

		public int DeviceIndex
		{
			get { return _deviceIndex; }
		}

		public string Name
		{
			get;
			private set;
		}

		public string GUID
		{
			get;
			private set;
		}

		public int NumModes
		{
			get { return _modes.Count; }
		}

		public int NumSettings
		{
			get { return _settings.Count; }
		}

		public int NumVideoInputs
		{
			get { return _videoInputs.Count; }
		}

		public Texture OutputTexture
		{
			get { if (_formatConverter != null && _formatConverter.ValidPicture) return _formatConverter.OutputTexture; return null; }
		}

		public int CurrentWidth
		{
			get { return _width; }
		}

		public int CurrentHeight
		{
			get { return _height; }
		}

		public string CurrentFormat
		{
			get { return _format; }
		}

		public string CurrentDeviceFormat
		{
			get { return _deviceFormat; }
		}

		public bool SupportsTransparency
		{
			get { return (_deviceFormat == "BGRA32" || _deviceFormat == "ARGB32" || _deviceFormat == "RGB32"); }
		}

		public float CurrentFrameRate
		{
			get { return _frameRate; }
		}

		public long CurrentFrameDurationHNS
		{
			get { return _frameDurationHNS; }
		}

		public bool IsRunning
		{
			get;
			private set;
		}

		public bool IsPaused
		{
			get;
			private set;
		}

		public bool IsPicture
		{
			get;
			private set;
		}

		public float CaptureFPS
		{
			get;
			private set;
		}

		public float DisplayFPS
		{
			get;
			private set;
		}

		public float CaptureFramesDropped
		{
			get;
			private set;
		}

		public int FramesTotal
		{
			get;
			private set;
		}

		public bool Deinterlace
		{
			get;
			set;
		}

		public bool IsConnected
		{
			get;
			private set;
		}

		public bool FlipX
		{
			get { return _flipX; }
			set { _flipX = value; if (_formatConverter != null) _formatConverter.FlipX = _flipX; }
		}

		public bool FlipY
		{
			get { return _flipY; }
			set { _flipY = value; if (_formatConverter != null) _formatConverter.FlipY = _isTopDown != _flipY; }
		}

		public bool AllowTransparency
		{
			get { return _allowTransparency; }
			set { _allowTransparency = value; if (_formatConverter != null) _formatConverter.AllowTransparency = _allowTransparency; }
		}		

		public bool UpdateHotSwap
		{
			get;
			set;
		}

		public bool UpdateFrameRates
		{
			get;
			set;
		}

		public bool UpdateSettings
		{
			get;
			set;
		}

		public AVProLiveCameraDevice(string name, string guid, int index)
		{
			IsRunning = false;
			IsPaused = true;
			IsPicture = false;
			IsConnected = true;
			UpdateHotSwap = false;
			UpdateFrameRates = false;
			UpdateSettings = false;
			Name = name;
			GUID = guid;
			_deviceIndex = index;
			_modes = new List<AVProLiveCameraDeviceMode>(64);
			_videoInputs = new List<string>(8);
			_settings = new List<AVProLiveCameraSettingBase>(16);
			_settingsByType = new Dictionary<int, AVProLiveCameraSettingBase>(16);
			_formatConverter = new AVProLiveCameraFormatConverter(_deviceIndex);
			EnumModes();
			EnumVideoInputs();
			EnumVideoSettings();
		}

		public void Dispose()
		{
			if (_formatConverter != null)
			{
				_formatConverter.Dispose();
				_formatConverter = null;
			}
		}

		public bool CanShowConfigWindow()
		{
			return AVProLiveCameraPlugin.HasDeviceConfigWindow(_deviceIndex);
		}

		public bool ShowConfigWindow()
		{
			return AVProLiveCameraPlugin.ShowDeviceConfigWindow(_deviceIndex);
		}

		public bool Start(AVProLiveCameraDeviceMode mode, int videoInputIndex = -1)
		{
			// Resolve the internal mode index
			int internalModeIndex = -1;
			if (mode != null)
			{
				internalModeIndex = mode.InternalIndex;
			}

			// Start the device
			if (AVProLiveCameraPlugin.StartDevice(_deviceIndex, internalModeIndex, videoInputIndex))
			{
				int modeIndex = -1;
				if (mode != null)
				{
					for (int i = 0; i < _modes.Count; i++)
					{
						if (_modes[i] == mode)
						{
							modeIndex = i;
							break;
						}
					}
				}
				Debug.Log("[AVProLiveCamera] Started device using mode index " + modeIndex + " (internal index " + internalModeIndex + ")");

				// Get format mode properties
				int width = AVProLiveCameraPlugin.GetWidth(_deviceIndex);
				int height = AVProLiveCameraPlugin.GetHeight(_deviceIndex);
				AVProLiveCameraPlugin.VideoFrameFormat format = (AVProLiveCameraPlugin.VideoFrameFormat)AVProLiveCameraPlugin.GetFormat(_deviceIndex);
				_width = width;
				_height = height;
				_format = format.ToString();
				_deviceFormat = AVProLiveCameraPlugin.GetDeviceFormat(_deviceIndex);
				_frameRate = AVProLiveCameraPlugin.GetFrameRate(_deviceIndex);
				_frameDurationHNS = AVProLiveCameraPlugin.GetFrameDurationHNS(_deviceIndex);

				// Validate properties
				if (width <= 0 || width > MaxVideoResolution || height <= 0 || height > MaxVideoResolution)
				{
					Debug.LogWarning("[AVProLiveCamera] invalid width or height");
					Close();
					return false;
				}

				// Create format converter
				_isTopDown = AVProLiveCameraPlugin.IsFrameTopDown(_deviceIndex);
				bool allowTransparency = (SupportsTransparency && AllowTransparency);
				if (!_formatConverter.Build(width, height, format, allowTransparency, _flipX, _isTopDown != _flipY, Deinterlace))
				{
					Debug.LogWarning("[AVProLiveCamera] unable to convert camera format");
					Close();
					return false;
				}

				// Run camera
				IsActive = true;
				IsRunning = false;
				IsPicture = false;
				IsPaused = true;
				_lastModeIndex = modeIndex;
				_lastVideoInputIndex = videoInputIndex;
				Play();

				return IsRunning;
			}

			Debug.LogWarning("[AVProLiveCamera] unable to start camera");
			Close();
			return false;
		}

		public bool Start(int modeIndex = -1, int videoInputIndex = -1)
		{
			AVProLiveCameraDeviceMode mode = null;
			if (modeIndex >= 0)
			{
				if (modeIndex < _modes.Count)
				{
					mode = _modes[modeIndex];
				}
				else
				{
					Debug.LogError("[AVProLiveCamera] Mode index out of range, using default resolution");
				}
			}

			return Start(mode, videoInputIndex);
		}

		public void SetVideoInputByIndex(int index)
		{
			AVProLiveCameraPlugin.SetVideoInputByIndex(_deviceIndex, index);
		}

		public void Play()
		{
			if (IsActive && (IsPaused || !IsRunning))
			{
				if (AVProLiveCameraPlugin.Play(_deviceIndex))
				{
					ResetDisplayFPS();
					IsPaused = false;
					IsRunning = true;
				}
				else
				{
					Debug.LogWarning("[AVProLiveCamera] failed to play camera");
				}
			}
		}

		public void Pause()
		{
			if (IsActive && IsRunning)
			{
				AVProLiveCameraPlugin.Pause(_deviceIndex);
				IsPaused = true;
			}
		}

		public void Stop()
		{
			if (IsActive && IsRunning)
			{
				AVProLiveCameraPlugin.Stop(_deviceIndex);
				IsRunning = false;
				IsPaused = true;
			}
		}

		private void Update_HotSwap()
		{
			bool isConnected = AVProLiveCameraPlugin.IsDeviceConnected(_deviceIndex);
			// If there is a change in the connection
			if (IsConnected != isConnected)
			{
				if (!isConnected)
				{
					Debug.Log("[AVProLiveCamera] device #" + _deviceIndex + " '" + Name + "' disconnected");
					Pause();
				}
				else
				{
					Debug.Log("[AVProLiveCamera] device #" + _deviceIndex + " '" + Name + "' reconnected");
					if (IsRunning)
					{
						Start(_lastModeIndex, _lastVideoInputIndex);
					}
				}

				IsConnected = isConnected;
			}
		}

		private void Update_FrameRates()
		{
			CaptureFPS = AVProLiveCameraPlugin.GetCaptureFrameRate(_deviceIndex);
			CaptureFramesDropped = AVProLiveCameraPlugin.GetCaptureFramesDropped(_deviceIndex);
		}

		public void Update(bool force)
		{
			if (UpdateHotSwap)
			{
				Update_HotSwap();
			}

			if (IsRunning)
			{
				if (UpdateFrameRates)
				{
					Update_FrameRates();
				}
				if (UpdateSettings)
				{
					Update_Settings();
				}
			}
		}

		public bool Render()
		{
			bool result = false;
			if (IsRunning)
			{
				if (_formatConverter != null)
				{
					if (_formatConverter.Update())
					{
						UpdateDisplayFPS();
						result = true;
					}
				}
			}
			return result;
		}

		public void Update_Settings()
		{
			for (int i = 0; i < _settings.Count; i++)
			{
				_settings[i].Update();
			}
		}

		protected void ResetDisplayFPS()
		{
			_frameCount = 0;
			FramesTotal = 0;
			DisplayFPS = 0.0f;
			_startFrameTime = 0.0f;
		}

		public void UpdateDisplayFPS()
		{
			_frameCount++;
			FramesTotal++;

			float timeNow = Time.realtimeSinceStartup;
			float timeDelta = timeNow - _startFrameTime;
			if (timeDelta >= 0.5f)
			{
				DisplayFPS = (float)_frameCount / timeDelta;
				_frameCount = 0;
				_startFrameTime = timeNow;
			}
		}

		public void Close()
		{
			ResetDisplayFPS();
			_width = _height = 0;
			_lastVideoInputIndex = _lastModeIndex = -1;
			_frameDurationHNS = 0;
			_frameRate = 0.0f;
			_format = _deviceFormat = string.Empty;
			IsRunning = false;
			IsPaused = true;
			AVProLiveCameraPlugin.StopDevice(_deviceIndex);
		}

		public string GetVideoInputName(int index)
		{
			string result = string.Empty;

			if (index >= 0 && index < _videoInputs.Count)
				result = _videoInputs[index];

			return result;
		}

		public AVProLiveCameraDeviceMode GetMode(int index)
		{
			AVProLiveCameraDeviceMode result = null;

			if (index >= 0 && index < _modes.Count)
			{
				result = _modes[index];
			}

			return result;
		}

		public AVProLiveCameraDeviceMode GetHighestResolutionMode(float minimumFrameRate)
		{
			AVProLiveCameraDeviceMode result = null;

			float highestRes = 0f;
			for (int i = 0; i < NumModes; i++)
			{
				AVProLiveCameraDeviceMode mode = GetMode(i);

				if (mode.FPS >= minimumFrameRate && (mode.Width * mode.Height > highestRes))
				{
					result = mode;
					highestRes = mode.Width * mode.Height;
				}
			}

			return result;
		}

		private static bool CompareMode(string internalMode, AVProLiveCameraPlugin.VideoFrameFormat pluginMode)
		{
			if (internalMode == pluginMode.ToString())
			{
				return true;
			}

			bool result = false;

			switch (pluginMode)
			{
				case AVProLiveCameraPlugin.VideoFrameFormat.RAW_BGRA32:
					result = (internalMode == "BGRA32" || internalMode == "ARGB32" || internalMode == "RGB32");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_YUY2:
					result = (internalMode == "YUV_YUY2");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_UYVY:
					result = (internalMode == "YUV_UYVY");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_YVYU:
					result = (internalMode == "YUV_YVYU");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_422_HDYC:
					result = (internalMode == "YUV_UYVY_HDYC");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_420_PLANAR_YV12:
					result = (internalMode == "YUV_PLANAR_YV12");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_420_PLANAR_I420:
					result = (internalMode == "YUV_PLANAR_I420");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.RAW_RGB24:
					result = (internalMode == "RGB24");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.RAW_MONO8:
					result = (internalMode == "MONO8" || internalMode == "Mono_Y800");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.RGB_10BPP:
					result = (internalMode == "RGB_10BPP" || internalMode == "RGBX_10BPP" || internalMode == "RGBXLE_10BPP");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.YUV_10BPP_V210:
					result = (internalMode == "YUV_10BPP_V210");
					break;
				case AVProLiveCameraPlugin.VideoFrameFormat.MPEG:
					result = (internalMode == "MJPG");
					break;
			}

			return result;
		}

		public AVProLiveCameraDeviceMode GetClosestMode(int width, int height, bool maintainAspectRatio, float frameRate, bool anyPixelFormat, bool transparentPixelFormat, AVProLiveCameraPlugin.VideoFrameFormat pixelFormat)
		{
			AVProLiveCameraDeviceMode result = null;

			if (width < -1 || height < -1)
				return result;

			List<AVProLiveCameraDeviceMode> bestModes = new List<AVProLiveCameraDeviceMode>();

			// Try to find exact match to resolution, or any resolution if width or height == -1
			{
				for (int i = 0; i < NumModes; i++)
				{
					AVProLiveCameraDeviceMode mode = GetMode(i);

					if ((width < 0 || mode.Width == width) && 
						(height < 0 || mode.Height == height))
					{
						bestModes.Add(mode);
					}
				}
			}

			// If we haven't found an exact match, find by closest area
			if (bestModes.Count == 0)
			{
				float aspect = (float)width * (float)height;
				int area = width * height;
				float lowestAreaDifference = float.MaxValue;
				for (int i = 0; i < NumModes; i++)
				{
					AVProLiveCameraDeviceMode mode = GetMode(i);

					// Maintain aspect ratio or not
					float modeAspect = (float)mode.Width / (float)mode.Height;
					bool consider = true;
					if (maintainAspectRatio && !Mathf.Approximately(modeAspect, aspect))
						consider = false;

					if (consider)
					{
						int modeArea = mode.Width * mode.Height;
						int areaDifference = Mathf.Abs(area - modeArea);
						if (areaDifference < lowestAreaDifference)
						{
							result = mode;
							lowestAreaDifference = areaDifference;
						}
					}
				}

				// Now that we know the closest resolution, collect all modes with that resolution
				if (result != null)
				{
					for (int i = 0; i < NumModes; i++)
					{
						AVProLiveCameraDeviceMode mode = GetMode(i);
						if (mode.Width == result.Width && mode.Height == result.Height)
						{
							bestModes.Add(mode);
						}
					}
					result = null;
				}
			}

			// Pick best based on pixel format or frame rate
			if (bestModes.Count > 0)
			{
				if (bestModes.Count == 1)
				{
					result = bestModes[0];
				}
				else
				{
					bool findHighestFrameRate = (frameRate <= 0f);
					if (findHighestFrameRate)
					{
						float highestFps = 0f;
						for (int i = 0; i < bestModes.Count; i++)
						{
							AVProLiveCameraDeviceMode mode = bestModes[i];
							if (mode.FPS > highestFps)
							{
								highestFps = mode.FPS;
							}
						}
						// Remove modes that didn't have the higest fps
						for (int i = 0; i < bestModes.Count; i++)
						{
							AVProLiveCameraDeviceMode mode = bestModes[i];
							if (mode.FPS < highestFps)
							{
								bestModes.RemoveAt(i);
								i = -1;
							}
						}
					}
					else
					{
						float lowestDelta = 1000f;
						float closestFps = 0f;
						// Find the closest FPS
						for (int i = 0; i < bestModes.Count; i++)
						{
							AVProLiveCameraDeviceMode mode = bestModes[i];
							float d = Mathf.Abs(mode.FPS - frameRate);
							if (d < lowestDelta)
							{
								closestFps = mode.FPS;
								lowestDelta = d;
							}
						}
						// Remove modes that have a different frameRate
						for (int i = 0; i < bestModes.Count; i++)
						{
							AVProLiveCameraDeviceMode mode = bestModes[i];
							if (mode.FPS != closestFps)
							{
								bestModes.RemoveAt(i);
								i = -1;
							}
						}
					}

					if (result == null)
					{
						if (bestModes.Count == 0)
						{
							result = null;
						}
						else if (bestModes.Count == 1)
						{
							result = bestModes[0];
						}
						else
						{
							if (transparentPixelFormat)
							{
								string[] bestFormats = { "ARGB32", "RGB32", "UNKNOWN" };
								int bestScore = 100;
								for (int i = 0; i < bestModes.Count; i++)
								{
									int index = System.Array.IndexOf<string>(bestFormats, bestModes[i].Format);
									if (index >= 0 && index < bestScore)
									{
										result = bestModes[i];
										bestScore = index;
									}
								}
							}
							else if (anyPixelFormat)
							{
								string[] bestFormats = { "YUV_UYVY_HDYC", "YUV_UYVY", "YUV_YVYU", "YUV_YUY2", "ARGB32", "RGB32", "RGB24", "MJPG", "UNKNOWN" };
								int bestScore = 100;
								for (int i = 0; i < bestModes.Count; i++)
								{
									int index = System.Array.IndexOf<string>(bestFormats, bestModes[i].Format);
									if (index >= 0 && index < bestScore)
									{
										result = bestModes[i];
										bestScore = index;
									}
								}
							}
							else
							{
								for (int i = 0; i < bestModes.Count; i++)
								{
									if (CompareMode(bestModes[i].Format, pixelFormat))
									{
										result = bestModes[i];
										break;
									}
								}
							}
						}
					}
				}
			}

			if (result != null)
			{
				//Debug.Log(string.Format("Selected mode {0}: {1}x{2} {3} {4}", result.Index, result.Width , result.Height, result.FPS, result.Format));
			}

			return result;
		}

		public AVProLiveCameraSettingBase GetVideoSettingByType(SettingsEnum type)
		{
			AVProLiveCameraSettingBase result = null;
			if (!_settingsByType.TryGetValue((int)type, out result))
			{
				result = null;
			}
			return result;
		}

		public AVProLiveCameraSettingBase GetVideoSettingByIndex(int index)
		{
			AVProLiveCameraSettingBase result = null;

			if (index >= 0 && index < _settings.Count)
				result = _settings[index];

			return result;
		}

		private void SortModes()
		{
			_modes.Sort(delegate (AVProLiveCameraDeviceMode x, AVProLiveCameraDeviceMode y)
			{
				int result = 0;

			// Sort by resolution
			if (x.Width * x.Height < y.Width * y.Height)
					result = -1;
				else if (y.Width * y.Height < x.Width * x.Height)
					result = 1;

			// Sort by framerate
			if (result == 0)
				{
					if (x.FPS < y.FPS)
						result = -1;
					else if (y.FPS < x.FPS)
						result = 1;
				}

			// Sort by format
			if (result == 0)
				{
					result = x.Format.CompareTo(y.Format);
				}

				return result;
			});
		}

		private void EnumModes()
		{
			int numModes = AVProLiveCameraPlugin.GetNumModes(_deviceIndex);
			for (int i = 0; i < numModes; i++)
			{
				int width, height;
				float fps;
				string format;
				if (AVProLiveCameraPlugin.GetModeInfo(_deviceIndex, i, out width, out height, out fps, out format))
				{
					AVProLiveCameraDeviceMode mode = new AVProLiveCameraDeviceMode(this, i, width, height, fps, format.ToString());
					_modes.Add(mode);
				}
			}
			SortModes();
		}

		private void EnumVideoInputs()
		{
			int numVideoInputs = AVProLiveCameraPlugin.GetNumVideoInputs(_deviceIndex);
			for (int i = 0; i < numVideoInputs; i++)
			{
				string name;
				if (AVProLiveCameraPlugin.GetVideoInputName(_deviceIndex, i, out name))
				{
					_videoInputs.Add(name);
				}
			}
		}

		private void EnumVideoSettings()
		{
			int numVideoSettings = AVProLiveCameraPlugin.GetNumDeviceVideoSettings(_deviceIndex);
			for (int i = 0; i < numVideoSettings; i++)
			{
				int settingType;
				int dataType;
				string name;
				bool canAutomatic;
				if (AVProLiveCameraPlugin.GetDeviceVideoSettingInfo(_deviceIndex, i, out settingType, out dataType, out name, out canAutomatic))
				{
					AVProLiveCameraSettingBase setting = null;

					// Data type is boolean
					if (dataType == 0)
					{
						bool defaultValue;
						bool currentValue;
						bool isAutomatic;
						if (AVProLiveCameraPlugin.GetDeviceVideoSettingBoolean(_deviceIndex, i, out defaultValue, out currentValue, out isAutomatic))
						{
							setting = new AVProLiveCameraSettingBoolean(_deviceIndex, i, settingType, name, canAutomatic, isAutomatic, defaultValue, currentValue);
						}
					}
					// Data type is float
					else if (dataType == 1)
					{
						bool isAutomatic;
						float defaultValue;
						float currentValue;
						float minValue, maxValue;
						if (AVProLiveCameraPlugin.GetDeviceVideoSettingFloat(_deviceIndex, i, out defaultValue, out currentValue, out minValue, out maxValue, out isAutomatic))
						{
							setting = new AVProLiveCameraSettingFloat(_deviceIndex, i, settingType, name, canAutomatic, isAutomatic, defaultValue, currentValue, minValue, maxValue);
						}
					}

					if (setting != null)
					{
						_settings.Add(setting);
						_settingsByType.Add(settingType, setting);
					}
				}
			}
		}
	}
}